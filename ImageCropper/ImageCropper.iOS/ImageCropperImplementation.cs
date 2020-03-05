using Bind_TOCropViewController;
using CoreGraphics;
using Foundation;
using Plugin.Media.Abstractions;
using Stormlion.ImageCropper.iOS;
using System;
using System.Diagnostics;
using UIKit;
using Xamarin.Forms;

namespace Stormlion.ImageCropper.iOS
{
    public class ImageCropperImplementation : IImageCropperWrapper 
    {
        public void ShowFromFile(ImageCropper imageCropper, string imageFile, OutputImageFormatType outputImageFormat)
        {
            UIImage image = UIImage.FromFile(imageFile);

            TOCropViewController cropViewController;

            if(imageCropper.CropShape == ImageCropper.CropShapeType.Oval)
            {
                cropViewController = new TOCropViewController(TOCropViewCroppingStyle.Circular, image);
            }
            else
            {
                cropViewController = new TOCropViewController(image);
            }

            /*if(imageCropper.AspectRatioX > 0 && imageCropper.AspectRatioY > 0)
            {
                cropViewController.AspectRatioPreset = TOCropViewControllerAspectRatioPreset.Custom;
                cropViewController.ResetAspectRatioEnabled = false;
                cropViewController.AspectRatioLockEnabled = true;
                cropViewController.CustomAspectRatio = new CGSize(imageCropper.AspectRatioX, imageCropper.AspectRatioY);
            }*/

            cropViewController.OnDidCropToRect = (outImage, cropRect, angle) =>
            {
                Finalize(imageCropper, outImage, outputImageFormat);
            };

            cropViewController.OnDidCropToCircleImage = (outImage, cropRect, angle) =>
            {
                Finalize(imageCropper, outImage, outputImageFormat);
            };

            cropViewController.OnDidFinishCancelled = (cancelled) =>
            {
                imageCropper.Faiure?.Invoke();
                UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController(true, null);
            };

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(cropViewController, true, null);

            if (!string.IsNullOrWhiteSpace(imageCropper.PageTitle) && cropViewController.TitleLabel != null)
            {
                UILabel titleLabel = cropViewController.TitleLabel;
                titleLabel.Text = imageCropper.PageTitle;
            }
        }

        private static async void Finalize(ImageCropper imageCropper, UIImage image, OutputImageFormatType outputImageFormat)
        {
            string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            string filename;
            NSData imgData;

            if (outputImageFormat == OutputImageFormatType.JPG)
            {

                filename = System.IO.Path.Combine(documentsDirectory, Guid.NewGuid().ToString() + ".jpg");
                imgData = image.AsJPEG();

            }
            else if (outputImageFormat == OutputImageFormatType.PNG)
            {
                filename = System.IO.Path.Combine(documentsDirectory, Guid.NewGuid().ToString() + ".png");
                imgData = image.AsPNG();
            }
            else
            {
                throw new ArgumentException("Unsupported image format");
            }

            NSError err;

            // small delay
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(100));
            if (imgData.Save(filename, false, out err))
            {
                imageCropper.Success?.Invoke(filename);
            }
            else
            {
                Debug.WriteLine("NOT saved as " + filename + " because" + err.LocalizedDescription);
                imageCropper.Faiure?.Invoke();
            }
            UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController(true, null);
        }
    }
}