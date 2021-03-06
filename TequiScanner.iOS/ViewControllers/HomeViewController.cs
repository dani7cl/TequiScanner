﻿using System;
using System.Collections.Generic;
using System.Drawing;
using AVFoundation;
using Foundation;
using TequiScanner.iOS.Components;
using TequiScanner.Shared.Model;
using TequiScanner.Shared.Services;
using UIKit;
namespace TequiScanner.iOS.ViewControllers
{
    public class HomeViewController : BaseViewController
    {
        private UIButton _takePhotoButton;
        private UIButton _galleryPhotoButton;
        private UIActivityIndicatorView _activityIndicator;

        private List<RecognitionResult> _resultList;

        private bool _fromGallery;

        public HomeViewController()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            View.BackgroundColor = Colors.BackgroundColor;
            _takePhotoButton = CreateButton("take photo".ToUpper(), TakePhotoButton_TouchUpInside);
            View.AddSubview(_takePhotoButton);

            View.AddConstraint(NSLayoutConstraint.Create(_takePhotoButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0));
            View.AddConstraint(NSLayoutConstraint.Create(_takePhotoButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1, 0));

            _galleryPhotoButton = CreateButton("browser gallery".ToUpper(), GalleryPhotoButton_TouchUpInside);

            View.AddSubview(_galleryPhotoButton);

            View.AddConstraint(NSLayoutConstraint.Create(_galleryPhotoButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0));
            View.AddConstraint(NSLayoutConstraint.Create(_galleryPhotoButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _takePhotoButton, NSLayoutAttribute.Bottom, 1, 10));

            _activityIndicator = new UIActivityIndicatorView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(_activityIndicator);

            View.AddConstraint(NSLayoutConstraint.Create(_activityIndicator, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0));
            View.AddConstraint(NSLayoutConstraint.Create(_activityIndicator, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1, 0));
            ToggleLoading(false);
        }

        private UIButton CreateButton(string text, EventHandler handler)
        {
            var button = new HighlightedButton(UIColor.Blue)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
            button.SetTitle(text, UIControlState.Normal);

            button.TouchUpInside += handler;
            button.Layer.CornerRadius = 6;
            return button;
        }

        private void TakePhotoButton_TouchUpInside(object sender, EventArgs e)
        {
            this._fromGallery = false;
            TakePicture();
        }

        private void GalleryPhotoButton_TouchUpInside(object sender, EventArgs e)
        {
            this._fromGallery = true;
            TakePicture();
        }

        private void TakePicture()
        {
            if (UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera) || _fromGallery)
            {
                UIImagePickerController imagePickerController = new UIImagePickerController()
                {
                    SourceType = UIImagePickerControllerSourceType.SavedPhotosAlbum,
                    ModalPresentationStyle = UIModalPresentationStyle.FullScreen
                };

                if (!_fromGallery)
                {
                    imagePickerController.SourceType = UIImagePickerControllerSourceType.Camera;
                    imagePickerController.ShowsCameraControls = true;
                    imagePickerController.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;

                }

                UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);
                this.PresentViewController(imagePickerController, true, null);
                imagePickerController.Canceled += CancelCamera_Handler;
                imagePickerController.FinishedPickingMedia += Camera_FinishedPickingMedia;
            }
        }

        private void CancelCamera_Handler(object sender, EventArgs e)
        {
            this.DismissViewController(true, null);
        }

        private async void Camera_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs mediaPicker)
        {
            ToggleLoading(true);
            _takePhotoButton.Hidden = true;
            _galleryPhotoButton.Hidden = true;
            UIImage originalImage = mediaPicker.Info[UIImagePickerController.OriginalImage] as UIImage;
            nfloat height = originalImage.PreferredPresentationSizeForItemProvider.Height;
            nfloat width = originalImage.PreferredPresentationSizeForItemProvider.Width;
            if(!_fromGallery) originalImage = MaxResizeImage(originalImage, (float)width/8, (float)height/8);
            
            NSData imgData = originalImage.AsJPEG(0.1f);
            byte[] dataBytesArray = imgData.ToArray();

            this.DismissModalViewController(true);

            try
            {
                RecognitionResult response = await new ComputerVisionService().RecognizeTextService(dataBytesArray);
                if (response != null)
                {

                    this.NavigationController.PushViewController(new TextDisplayController(response, height, width, 0), true);
                }
                else
                {
                    UIAlertView alert = new UIAlertView()
                    {
                        Title = "Error",
                        Message = "API Fail"
                    };
                    alert.AddButton("Cancel");
                    alert.Show();
                }
            }
            finally
            {
                _takePhotoButton.Hidden = false;
                _galleryPhotoButton.Hidden = false;
                ToggleLoading(false);
            }
        }

        UIImage MaxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            if (maxResizeFactor > 1) return sourceImage;
            var width = maxResizeFactor * sourceSize.Width;
            var height = maxResizeFactor * sourceSize.Height;
            UIGraphics.BeginImageContext(new SizeF((float)width, (float)height));
            sourceImage.Draw(new RectangleF(0, 0, (float)width, (float)height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }


        private void ToggleLoading(bool showLoading)
        {
            if (showLoading)
            {
                _activityIndicator.Hidden = false;
                _activityIndicator.StartAnimating();
            }
            else
            {
                _activityIndicator.Hidden = true;
                _activityIndicator.StopAnimating();
            }
        }
    }
}
