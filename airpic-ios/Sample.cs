// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and

using System;
using Foundation;
using UIKit;
using Poupou.AirPlay;

namespace AirPicDemo {

	public class ImageViewController : UIViewController	{

		UIPopoverController popup;
		UIImageView image_view;

		public ImageViewController ()
		{
			Title = "AirPic Demo";
			AirPlayBrowser.Enabled = true;

			var bounds = UIScreen.MainScreen.Bounds;
			image_view = new UIImageView (bounds);
			image_view.Image = UIImage.FromFile ("687px-Leontopithecus_rosalia_-_Copenhagen_Zoo_-_DSC09082.JPG");
			image_view.Image.Scale (bounds.Size);
			Add (image_view);

			UIBarButtonItem action = null;
			action = new UIBarButtonItem (UIBarButtonSystemItem.Action, delegate {
				if (image_view.Image == null)
					return;
				// UIActivity is only for iOS6+ but that should not limit us :-)
				if (UIDevice.CurrentDevice.CheckSystemVersion (6,0)) {
					UIActivityViewController a = new UIActivityViewController (new [] { image_view.Image }, 
						UIAirPlayActivity.GetCurrentActivities ());
					if (AppDelegate.RunningOnIPad) {
						popup = new UIPopoverController (a);
						popup.PresentFromBarButtonItem (action, UIPopoverArrowDirection.Up, true);
					} else {
						PresentViewController (a, true, null);
					}
				} else {
					var devices = AirPlayBrowser.GetDeviceNames ();
					UIActionSheet a = new UIActionSheet (null, null, "Cancel", null, devices);
					a.Clicked += (object sender, UIButtonEventArgs e) => {
						nint index = e.ButtonIndex;
						// ignore Cancel button
						if (index < devices.Length) {
							var device = AirPlayBrowser.GetDevice (devices [index]);
							if (device != null) // they can disappear anytime
								device.SendTo (image_view.Image, null);
						}
					};
					a.ShowFrom (NavigationItem.RightBarButtonItem, true);
				}
			});
			NavigationItem.RightBarButtonItem = action;
		}
	}

	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate {
		
		UIWindow window;
		
		public static bool RunningOnIPad { get; private set; }
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			RunningOnIPad = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
			
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			window.RootViewController = new UINavigationController (new ImageViewController ());
			window.MakeKeyAndVisible ();
			return true;
		}
		
		public class Application
		{
			static void Main (string[] args)
			{
				UIApplication.Main (args, null, "AppDelegate");
			}
		}
	}
}