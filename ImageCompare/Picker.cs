using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ImageCompare
{
	public struct PointInfo
	{
		public Color color;
		public double leftSig;
		public double rightSig;
		public double upSig;
		public double downSig;
	}

	public partial class Picker : Form
	{
		private int currentFileIndex = 0; //Image A index
		private int compareFileIndex = 1; //Image B index
		private String pooldir; //Directory we are going through
		private String lowresdir; //Directory to place lower-resolution files
		private List<String> filePool; //All files we will be looking at
		private List<List<PointInfo>> pointInfo; //Pixels (and related info) from each image, derrived from same % based coordinates
		private List<List<long>> dimensions; //Dimensions of image
		private Task compareTask;

		private int userSelection = 0; //0 = waiting, 1 = similar, 2 = different
		private bool finished = false; //Are we done checking
		private double ncomps = 0; //Number of images comparisons done

		public Picker(String directory = "")
		{
			InitializeComponent();

			//No/invalid directory, ask user for directory
			while (!System.IO.Directory.Exists(directory))
			{
				folderBrowserDialog1.Description = "Select a directory with images to compare.";
				DialogResult r = folderBrowserDialog1.ShowDialog();
				directory = folderBrowserDialog1.SelectedPath;
			}
			//populate filePool (Dir string should be dynamic)
			filePool = new List<String>(Directory.GetFiles(directory));
			pointInfo = new List<List<PointInfo>>();
			dimensions = new List<List<long>>();
			for (int i = 0; i < filePool.Count; i++)
			{
				pointInfo.Add(new List<PointInfo>());
				dimensions.Add(new List<long>());
				dimensions[i].Add(1); //W
				dimensions[i].Add(1); //H
			}
			pooldir = directory;
			lowresdir = System.IO.Path.Combine(pooldir, "lowresdup");
			Console.WriteLine("Pool of " + filePool.Count + " files.");

			System.IO.Directory.CreateDirectory(lowresdir);
			compareTask = new Task(DoLoop);
			compareTask.Start();
		}

		private void ButtonMatch_Click(object sender, EventArgs e)
		{
			if (userSelection == 0)
			{
				Console.WriteLine("Pictures match");
				userSelection = 1;
			}
		}

		private void ButtonDiffer_Click(object sender, EventArgs e)
		{
			if (userSelection == 0)
			{
				Console.WriteLine("Pictures differ");
				userSelection = 2;
			}
		}

		private void MoveToLowRes(int poolIndex)
		{
			String file = filePool[poolIndex];
			String destPath = System.IO.Path.Combine(lowresdir, Path.GetFileName(file));
			if (!File.Exists(destPath))
			{
				//Copy to dest
				System.IO.File.Copy(file, destPath);
				filePool[poolIndex] = "";
				//Garbage Collect
				System.GC.Collect();  
				System.GC.WaitForPendingFinalizers();
				//Delete original
				File.Delete(file);
				//Remove from pool, randomPixels, and ratios
				filePool.RemoveAt(poolIndex);
				pointInfo.RemoveAt(poolIndex);
				dimensions.RemoveAt(poolIndex);
			} else
			{
				Console.WriteLine("Could not move \"" + file + "\"; File with same name exists in target directory.");
			}
		}

		private void DoLoop()
		{
			if (currentFileIndex >= filePool.Count)
			{
				finished = true;
				return; //No more files
			}
			finished = false;

			Invoke((MethodInvoker)delegate {
				CenterInfo.Text = "Loading select pixels from each image...";
				ProgressBarActive.Visible = true;
			});
			GetRandomPatternPixels(); //Get color mapping for `AreDifferentFast` function
			Invoke((MethodInvoker)delegate {
				CenterInfo.Text = "Beginning comparison";
				ProgressBarActive.Visible = false;
			});

			while (!finished)
			{
				//Compare current images
				bool differ = AreDifferentFast(currentFileIndex, compareFileIndex);
				ncomps++; //Track number of comparisons

				//If they do not differ, save larger resolution one
				if (!differ)
				{
					//Move lower resolution one to given folder "./lowres/"
					//Keep larger resolution one as-is

					Point ImageARes = new Point(x: LeftImage.Width, y: LeftImage.Height);
					Point ImageBRes = new Point(x: RightImage.Width, y: RightImage.Height);
					int AgtB = (ImageARes.X * ImageARes.Y) - (ImageBRes.X * ImageBRes.Y); //ImageARes >= ImageBRes
					//If ImageA is lower, reset compareFileIndex (ImageB index) and move ImageA
					if (AgtB < 0) //"A < B"
					{
						//Move ImageA
						MoveToLowRes(currentFileIndex); //Move this index, from pool, to the lowres folder

						compareFileIndex = currentFileIndex + 1;
					}
					else
					//If ImageB is lower (or same res), don't increment compareFileIndex and move ImageB
					if (AgtB > 0) {
						//Move ImageB
						MoveToLowRes(compareFileIndex);
					} else
					{
						//Equal resolution (exactly), keep older
						DateTime timeA = File.GetCreationTimeUtc(filePool[currentFileIndex]);
						DateTime timeB = File.GetCreationTimeUtc(filePool[compareFileIndex]);

						if (timeA < timeB)
						{
							//image A older
							//Move ImageA
							MoveToLowRes(currentFileIndex); //Move this index, from pool, to the lowres folder

							compareFileIndex = currentFileIndex + 1;
						} else
						{
							//Move ImageB
							MoveToLowRes(compareFileIndex);
						}
					}
				}
				else
				{
					//Increment to next (assumes indexes have not changed(true for different files))
					compareFileIndex++;
					if (compareFileIndex >= filePool.Count)
					{
						currentFileIndex++;
						compareFileIndex = currentFileIndex + 1;
						if (currentFileIndex >= filePool.Count - 1)
						{
							break; //No more files
						}
					}
				}

				//Set progress bar
				{
					//n*(n-1)/2 is total
					//comparisons is current progress
					int progress = (int)Math.Floor(ncomps / ((double)filePool.Count * (filePool.Count - 1) / 2) * 100);
					if (filePool.Count == 1)
					{
						progress = 100;
					}
					Invoke((MethodInvoker)delegate {
						ProgressBarPassive.Value = Math.Min(progress, 100);
					});
				}
			}

			//We have completed our task
			finished = true;
			Invoke((MethodInvoker)delegate {
				CenterInfo.Text = "Finished";
				ProgressBarPassive.Value = 100;
			});
		}

		//For each image in pool, get 100 pixels from each image (random, predefined pattern)
		private void GetRandomPatternPixels()
		{
			//Generate pattern
			List<PointF> points = new List<PointF>();
			Random rng = new Random();
			for (int i = 0; i < 20 * 5; i++)
			{
				points.Add(new PointF(x: (float)rng.NextDouble(), y: (float)rng.NextDouble()));
			}

			//For each image
			for (int i = 0; i < filePool.Count; i++)
			{
				Image img;
				try
				{
					//Load image
					img = Image.FromFile(filePool[i]);
					//Save ratio (width / height)
					dimensions[i][0] = img.Width;
					dimensions[i][1] = img.Height;
					//Construct PointInfo list
					pointInfo[i] = new List<PointInfo>(0);
					//Get point info (pixel color, nearest in any (4) direction with significant border)
					foreach (PointF p in points)
					{
						PointInfo pi = new PointInfo();
						PointF lp = new PointF(x: p.X * img.Width, y: p.Y * img.Height); //Local, floating point, pixel
						Point lpi = new Point(x: (int)Math.Floor(lp.X), y: (int)Math.Floor(lp.Y));

						//Get point color
						{
							//Get nearest 4 pixels
							Point pixelff = new Point(x: (int)Math.Floor(lp.X), y: (int)Math.Floor(lp.Y));
							Point pixelfc = new Point(x: (int)Math.Floor(lp.X), y: (int)Math.Ceiling(lp.Y));
							Point pixelcf = new Point(x: (int)Math.Ceiling(lp.X), y: (int)Math.Floor(lp.Y));
							Point pixelcc = new Point(x: (int)Math.Ceiling(lp.X), y: (int)Math.Ceiling(lp.Y));
							//Make sure none are out of bounds
							pixelfc.Y = Math.Min(pixelfc.Y, img.Height - 1);
							pixelcf.X = Math.Min(pixelcf.X, img.Width - 1);
							pixelcc.X = Math.Min(pixelcc.X, img.Width - 1);
							pixelcc.Y = Math.Min(pixelcc.Y, img.Height - 1);

							//Get colors
							Color colorff = ((Bitmap)img).GetPixel(pixelff.X, pixelff.Y);
							Color colorfc = ((Bitmap)img).GetPixel(pixelfc.X, pixelfc.Y);
							Color colorcf = ((Bitmap)img).GetPixel(pixelcf.X, pixelcf.Y);
							Color colorcc = ((Bitmap)img).GetPixel(pixelcc.X, pixelcc.Y);

							//Calculate color
							//Color of top and bottom horizontal
							Color th = Color.FromArgb((colorfc.A + colorcc.A) / 2, (colorfc.R + colorcc.R) / 2, (colorfc.G + colorcc.G) / 2, (colorfc.B + colorcc.B) / 2);
							Color bh = Color.FromArgb((colorff.A + colorcf.A) / 2, (colorff.R + colorcf.R) / 2, (colorff.G + colorcf.G) / 2, (colorff.B + colorcf.B) / 2);
							//Color between horizontal colors is what we use
							pi.color = Color.FromArgb((th.A + bh.A) / 2, (th.R + bh.R) / 2, (th.G + bh.G) / 2, (th.B + bh.B) / 2);
						}

						//Get nearest border in all directions
						{
							//Up
							{ 
								Point lastPixelCoord = new Point(x: lpi.X, y: lpi.Y);
								Color lastPixel = ((Bitmap)img).GetPixel(lpi.X, lpi.Y);
								pi.upSig = img.Height - lastPixelCoord.Y;
								for (int y = lpi.Y; y < img.Height; y++)
								{
									//If total difference integer is > 127, border has been hit
									Color thisPixel = ((Bitmap)img).GetPixel(lpi.X, y);
									int diff = 0;
									diff += Math.Abs(lastPixel.A - thisPixel.A);
									diff += Math.Abs(lastPixel.R - thisPixel.R);
									diff += Math.Abs(lastPixel.G - thisPixel.G);
									diff += Math.Abs(lastPixel.B - thisPixel.B);
									if (diff > (128 - 1))
									{
										//Border hit, record y
										pi.upSig = y - lpi.Y;
										break;
									}
								}
							}
							pi.upSig /= img.Height; //Make [0-1]

							//Down
							{
								Point lastPixelCoord = new Point(x: lpi.X, y: lpi.Y);
								Color lastPixel = ((Bitmap)img).GetPixel(lpi.X, lpi.Y);
								pi.downSig = lastPixelCoord.Y;
								for (int y = lpi.Y; y >= 0; y--)
								{
									//If total difference integer is > 127, border has been hit
									Color thisPixel = ((Bitmap)img).GetPixel(lpi.X, y);
									int diff = 0;
									diff += Math.Abs(lastPixel.A - thisPixel.A);
									diff += Math.Abs(lastPixel.R - thisPixel.R);
									diff += Math.Abs(lastPixel.G - thisPixel.G);
									diff += Math.Abs(lastPixel.B - thisPixel.B);
									if (diff > (128 - 1))
									{
										//Border hit, record y
										pi.downSig = lpi.Y - y;
										break;
									}
								}
							}
							pi.downSig /= img.Height; //Make [0-1]

							//Left
							{
								Point lastPixelCoord = new Point(x: lpi.X, y: lpi.Y);
								Color lastPixel = ((Bitmap)img).GetPixel(lpi.X, lpi.Y);
								pi.leftSig = lastPixelCoord.X;
								for (int x = lpi.X; x >= 0; x--)
								{
									//If total difference integer is > 127, border has been hit
									Color thisPixel = ((Bitmap)img).GetPixel(x, lpi.Y);
									int diff = 0;
									diff += Math.Abs(lastPixel.A - thisPixel.A);
									diff += Math.Abs(lastPixel.R - thisPixel.R);
									diff += Math.Abs(lastPixel.G - thisPixel.G);
									diff += Math.Abs(lastPixel.B - thisPixel.B);
									if (diff > (128 - 1))
									{
										//Border hit, record y
										pi.leftSig = lpi.X - x;
										break;
									}
								}
							}
							pi.leftSig /= img.Width; //Make [0-1]

							//Right
							{
								Point lastPixelCoord = new Point(x: lpi.X, y: lpi.Y);
								Color lastPixel = ((Bitmap)img).GetPixel(lpi.X, lpi.Y);
								pi.rightSig = img.Width - lastPixelCoord.X;
								for (int x = lpi.X; x < img.Width; x++)
								{
									//If total difference integer is > 127, border has been hit
									Color thisPixel = ((Bitmap)img).GetPixel(x, lpi.Y);
									int diff = 0;
									diff += Math.Abs(lastPixel.A - thisPixel.A);
									diff += Math.Abs(lastPixel.R - thisPixel.R);
									diff += Math.Abs(lastPixel.G - thisPixel.G);
									diff += Math.Abs(lastPixel.B - thisPixel.B);
									if (diff > (128 - 1))
									{
										//Border hit, record y
										pi.rightSig = x - lpi.X;
										break;
									}
								}
							}
							pi.rightSig /= img.Width; //Make [0-1]
						}

						pointInfo[i].Add(pi);
					}
				} catch (Exception e)
				{
					//Failed to load image
					pointInfo[i] = new List<PointInfo>(0);
				}

				//Update progress bar
				Invoke((MethodInvoker)delegate {
					ProgressBarActive.Value = (int)Math.Floor((double)i / filePool.Count * 100);
				});
			}
			
			//Relevant pixels have been obtained from all images
		}

		//Are given two images different using the pre-defined pixel coords
		private bool AreDifferentFast(int indexA, int indexB)
		{
			if (pointInfo[indexA].Count != pointInfo[indexB].Count)
			{
				//Can't compare
				//Most likely, one or both counts are 0
				//which indicate the image could not be loaded
				//Declare as different images to be safe
				return true;
			}

			//Do comparison
			double similarity = 0;// randomPixels[indexA].Count * 4;
			for (int i = 0; i < pointInfo[indexA].Count; i++)
			{
				Color colorA = pointInfo[indexA][i].color;
				Color colorB = pointInfo[indexB][i].color;

				//Compare pixel colors, record difference
				double rdiff = (0xFF - Math.Abs(colorA.R - colorB.R)) / (double)0xFF;
				double gdiff = (0xFF - Math.Abs(colorA.G - colorB.G)) / (double)0xFF;
				double bdiff = (0xFF - Math.Abs(colorA.B - colorB.B)) / (double)0xFF;
				double adiff = (0xFF - Math.Abs(colorA.A - colorB.A)) / (double)0xFF; //(Usually 1 (Identical))
				//All diffs above range from 0 to 1
				double cdiff = rdiff * gdiff * bdiff * adiff;
				similarity += cdiff;

				PointInfo piA = pointInfo[indexA][i];
				PointInfo piB = pointInfo[indexB][i];

				//Compare border distances
				double updiff = 1 - Math.Abs(piA.upSig - piB.upSig);
				double dodiff = 1 - Math.Abs(piA.downSig - piB.downSig);
				double lediff = 1 - Math.Abs(piA.leftSig - piB.leftSig);
				double ridiff = 1 - Math.Abs(piA.rightSig - piB.rightSig);
				//Max differences can be 1
				double ddiff = updiff * dodiff * lediff * ridiff;
				similarity += ddiff;
			}
			similarity /= pointInfo[indexA].Count * 2; //Make into [0-1] from [0-2n]

			//Return based on if similarity is past threshold
			Invoke((MethodInvoker)delegate {
				CenterInfo.Text = "Similarity: " + Math.Round(similarity, 4);
			});

			//If pixel colors differ too much, OR the ratio differs too much, assume different
			double ratioA = (double)dimensions[indexA][0] / dimensions[indexA][1];
			double ratioB = (double)dimensions[indexB][0] / dimensions[indexB][1];
			double ratioDiff = Math.Abs(ratioA - ratioB);
			bool differ = similarity < 0.90 || ratioDiff >= 0.1;
			Console.WriteLine(Path.GetFileName(filePool[indexA]) + " vs " + Path.GetFileName(filePool[indexB]));
			Console.WriteLine("Similarity: " + similarity + ", ratio diff: " + ratioDiff);
			Console.WriteLine("Ratio/Similarity: " + ratioDiff / similarity);

			long pixelsA = dimensions[indexA][0] * dimensions[indexA][1];
			long pixelsB = dimensions[indexB][0] * dimensions[indexB][1];
			double pixelRatio = (double)Math.Min(pixelsA, pixelsB) / Math.Max(pixelsA, pixelsB);
			//Half the resolution (exactly 1/4 the pixel count) caused a 0.08 drop in similiarity value
			//We account for this by expecting similiarity to be 1 for identically sized photos, and scaling down from 1 as resolution difference increases between the two
			differ = (ratioDiff / similarity) >= 0.03125 || ratioDiff >= 0.1 || similarity < (1 - (1 - pixelRatio) * 0.1);

			//We think images are similar and they are same dimensions (we can compare them automatically)
			if (!differ && dimensions[indexA].SequenceEqual(dimensions[indexB]))
			{
				//Load and show images
				Image ImageA, ImageB;
				try
				{
					ImageA = Image.FromFile(filePool[indexA]);
					ImageB = Image.FromFile(filePool[indexB]);
				}
				catch (Exception e)
				{
					//Assume different since we do nothing when thats the case
					return true;
				}
				Bitmap bitmapA = new Bitmap(ImageA);
				Bitmap bitmapB = new Bitmap(ImageB);
				long totalPixels = (dimensions[indexA][0] * dimensions[indexA][1]);
				Invoke((MethodInvoker)delegate {
					LeftImage.Image = ImageA;
					RightImage.Image = ImageB;
					LeftInfo.Text = Path.GetFileName(filePool[indexA]) + "\nRatio: " + ratioA + "\nRes: " + ImageA.Width + "x" + ImageA.Height;
					RightInfo.Text = Path.GetFileName(filePool[indexB]) + "\nRatio: " + ratioB + "\nRes: " + ImageB.Width + "x" + ImageB.Height;
					CenterInfo.Text = "Comparing pixel by pixel (0%)";
					ProgressBarActive.Visible = true;
				});

				//Split up work to multiple threads
				bool mismatchFound = false;
				int threadcount = 4;
				List<Thread> threads = new List<Thread>();
				long pixelsCompared = 0;
				for (int i = 0; i < threadcount; i++)
				{
					//Start thread
					long startCol = i / threadcount * dimensions[indexA][0];
					long endCol = (i + 1) / threadcount * dimensions[indexA][0];

					Thread t = new Thread(new ThreadStart(() => {
						for (long x = startCol; x < endCol && mismatchFound == false; x++)
						{
							for (long y = 0; y < dimensions[indexA][1] && mismatchFound == false; y++)
							{
								if (bitmapA.GetPixel((int)x, (int)y) != bitmapB.GetPixel((int)x, (int)y))
								{
									//No match, mismatch found
									mismatchFound = true;
								}
								pixelsCompared++;
							}
						}
					}));
					t.Start();

					threads.Add(t);
				}
				while (pixelsCompared < totalPixels && mismatchFound == false)
				{
					long progress = pixelsCompared * 100 / totalPixels;
					Invoke((MethodInvoker)delegate {
						CenterInfo.Text = "Comparing pixel by pixel (" + progress + "%)";
						ProgressBarActive.Value = (int)progress;
					});
				}
				//Either all threads are finished OR mismatch was found
				//Join threads back together
				for (int i = 0; i < threadcount; i++)
				{
					threads[i].Join();
				}
				

				Invoke((MethodInvoker)delegate {
					LeftImage.Image = null;
					RightImage.Image = null;
					LeftInfo.Text = "Wait...";
					RightInfo.Text = "Wait...";
					ProgressBarActive.Visible = false;
				});

				return mismatchFound;
			}
			//We think images are similar and they are different resolutions, ask user to confirm
			else if (!differ)
			{
				//Users needs to be shown images
				//Load and show (Hide progress bar)
				Image ImageA, ImageB;
				try
				{
					ImageA = Image.FromFile(filePool[indexA]);
					ImageB = Image.FromFile(filePool[indexB]);
				}
				catch (Exception e)
				{
					//Assume different since we do nothing when thats the case
					return true;
				}
				Invoke((MethodInvoker)delegate {
					LeftImage.Image = ImageA;
					RightImage.Image = ImageB;
					LeftInfo.Text = Path.GetFileName(filePool[indexA]) + "\nRatio: " + ratioA + "\nRes: " + ImageA.Width + "x" + ImageA.Height;
					RightInfo.Text = Path.GetFileName(filePool[indexB]) + "\nRatio: " + ratioB + "\nRes: " + ImageB.Width + "x" + ImageB.Height;
				});

				//Wait for user response
				userSelection = 0;
				while (userSelection == 0) { }

				if (userSelection == 1)
				{
					//User also says they are similar
					differ = false;
				}
				else
				{
					//Users says they are different
					differ = true;
				}

				//Clear pictures to show we are no longer asking for input
				Invoke((MethodInvoker)delegate {
					LeftImage.Image = null;
					RightImage.Image = null;
					LeftInfo.Text = "Wait...";
					RightInfo.Text = "Wait...";
				});
			}

			return differ;
		}

		//Are given two images different (random points for every compare)
		private bool AreDifferent(int indexA, int indexB)
		{
			Image ImageA, ImageB;
			try
			{
				ImageA = Image.FromFile(filePool[indexA]);
				ImageB = Image.FromFile(filePool[indexB]);
			} catch (Exception e)
			{
				//Assume different since we do nothing when thats the case
				return true;
			}
			//Garbage collect the old images
			//System.GC.Collect();
			//System.GC.WaitForPendingFinalizers();
			//Display images as WE (computer) go through them
			Invoke((MethodInvoker)delegate {
				LeftImage.Image = ImageA;
				RightImage.Image = ImageB;
				LeftInfo.Text = filePool[indexA];
				RightInfo.Text = filePool[indexB];
			});

			//Select n random points (by % width/height) and compare pixel colors at each
			double similarity = 1;
			Random rng = new Random();
			for (int n = 0; n < 20 * 5; n++)
			{
				double precentX = rng.NextDouble();
				double precentY = rng.NextDouble();
				Point pointA = new Point(x: (int)Math.Floor(precentX * ImageA.Width), y: (int)Math.Floor(precentY * ImageA.Height));
				Point pointB = new Point(x: (int)Math.Floor(precentX * ImageB.Width), y: (int)Math.Floor(precentY * ImageB.Height));

				//Get color of pixels
				Color colorA = ((Bitmap)ImageA).GetPixel(pointA.X, pointA.Y);
				Color colorB = ((Bitmap)ImageB).GetPixel(pointB.X, pointB.Y);

				//Compare pixel colors, record difference
				double rdiff = (0xFF - Math.Abs(colorA.R - colorB.R)) / (double)0xFF;
				double gdiff = (0xFF - Math.Abs(colorA.G - colorB.G)) / (double)0xFF; //107 - 244 = -137 //(255 - 137) / 255
				double bdiff = (0xFF - Math.Abs(colorA.B - colorB.B)) / (double)0xFF;
				double adiff = (0xFF - Math.Abs(colorA.A - colorB.A)) / (double)0xFF; //(Usually 1 (Identical))
				//All diffs above range from 0 to 1
				similarity *= rdiff * gdiff * bdiff * adiff;
			}

			//Return based on if similarity is past threshold
			Invoke((MethodInvoker)delegate {
				CenterInfo.Text = "Similarity: " + Math.Round(similarity, 4);
			});

			bool differ = similarity < 0.9;

			//We think images are similar, ask user to confirm
			if (!differ)
			{
				userSelection = 0;
				while (userSelection == 0) { }

				if (userSelection == 1)
				{
					//User also says they are similar
					differ = false;
				} else
				{
					//Users says they are different
					differ = true;
				}
			}

			return differ;
		}
	}
}
