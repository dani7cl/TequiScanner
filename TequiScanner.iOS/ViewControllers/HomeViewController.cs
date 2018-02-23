﻿using System;
using AVFoundation;
using Foundation;
using Photos;
using TequiScanner.iOS.Components;
using UIKit;
namespace TequiScanner.iOS.ViewControllers
{
    public class HomeViewController : BaseViewController
    {

        private UIButton _takePhotoButton;
        private UIButton _galleryPhotoButton;

        public HomeViewController()
        {
        }


        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            SetupUI();

        }


        private void SetupUI() {

            View.BackgroundColor = UIColor.FromRGB(230, 230, 230); 
            _takePhotoButton = new HighlightedButton(UIColor.Green)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _takePhotoButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
            _takePhotoButton.SetTitle("TAKE NEW PHOTO", UIControlState.Normal);

            _takePhotoButton.TouchUpInside += TakePhotoButton_TouchUpInside;
            _takePhotoButton.Layer.CornerRadius = 5;
            View.AddSubview(_takePhotoButton);

            View.AddConstraint(NSLayoutConstraint.Create(_takePhotoButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0));
            View.AddConstraint(NSLayoutConstraint.Create(_takePhotoButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1, 0));

            _galleryPhotoButton = new HighlightedButton(UIColor.Green)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _galleryPhotoButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);

            _galleryPhotoButton.SetTitle("BROWSE GALLERY", UIControlState.Normal);
            _galleryPhotoButton.TouchUpInside += GalleryPhotoButton_TouchUpInside;

            View.AddSubview(_galleryPhotoButton);

            View.AddConstraint(NSLayoutConstraint.Create(_galleryPhotoButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0));
            View.AddConstraint(NSLayoutConstraint.Create(_galleryPhotoButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal,_takePhotoButton, NSLayoutAttribute.Bottom, 1, 10));
        }

        private void TakePhotoButton_TouchUpInside(object sender, EventArgs e) {

            TakePicture();
        }

        private void GalleryPhotoButton_TouchUpInside(object sender, EventArgs e)
        {
            TakePicture(true);
        }

        private void TakePicture(bool fromGallery = false) {

            if (UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera))
            {
                UIImagePickerController imagePickerController = new UIImagePickerController()
                {
                    SourceType = UIImagePickerControllerSourceType.Camera,
                    ShowsCameraControls = true,
                    CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo,
                    ModalPresentationStyle = UIModalPresentationStyle.FullScreen,
                };
                if (fromGallery) imagePickerController.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
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

        private void Camera_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs mediaPicker)
        {

            UIImage originalImage = mediaPicker.Info[UIImagePickerController.OriginalImage] as UIImage;

            NSData imgData = originalImage.AsJPEG(0.5f);

            byte[] dataBytesArray = imgData.ToArray();


            this.DismissModalViewController(true);

            this.NavigationController.PushViewController(new TextDisplayController(), true);
        }


    }
}