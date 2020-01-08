﻿using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Stormlion.ImageCropper
{
    public enum OutputImageFormatType { PNG, JPG }

    public class ImageCropper
    {
        public static ImageCropper Current { get; set; }

        public ImageCropper()
        {
            Current = this;
        }

        public enum CropShapeType
        {
            Rectangle,
            Oval
        };

        public OutputImageFormatType OutputImageFormat { get; set; } = OutputImageFormatType.JPG;

        public CropShapeType CropShape { get; set; } = CropShapeType.Rectangle;

        public int AspectRatioX { get; set; } = 0;

        public int AspectRatioY { get; set; } = 0;

        public string PageTitle { get; set; } = null;

        public string SelectSourceTitle { get; set; } = "Select source";

        public string TakePhotoTitle { get; set; } = "Take Photo";

        public string PhotoLibraryTitle { get; set; } = "Photo Library";

        public string CancelButtonTitle { get; set; } = "Cancel";

        public Action<string> Success { get; set; }

        public Action Faiure { get; set; }

        public async void Show(Page page, string imageFile = null)
        {
            if(imageFile == null)
            {
                await CrossMedia.Current.Initialize();

                MediaFile file;

                string action = await page.DisplayActionSheet(SelectSourceTitle, CancelButtonTitle, null, TakePhotoTitle, PhotoLibraryTitle);
                if (action == TakePhotoTitle)
                {
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        await page.DisplayAlert("No Camera", ":( No camera available.", "OK");
                        Faiure?.Invoke();
                        return;
                    }

                     file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions());
                }
                else if(action == PhotoLibraryTitle)
                {
                    if(!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        await page.DisplayAlert("Error", "This device is not supported to pick photo.", "OK");
                        Faiure?.Invoke();
                        return;
                    }

                    file = await CrossMedia.Current.PickPhotoAsync();
                }
                else
                {
                    Faiure?.Invoke();
                    return;
                }

                if (file == null)
                {
                    Faiure?.Invoke();
                    return;
                }

                imageFile = file.Path;
            }

            // small delay
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            DependencyService.Get<IImageCropperWrapper>().ShowFromFile(this, imageFile, OutputImageFormat);
        }
    }
}
