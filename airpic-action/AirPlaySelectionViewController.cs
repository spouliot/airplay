// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2014 Xamarin Inc.
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
using MobileCoreServices;
using UIKit;

using Poupou.AirPlay;

namespace AirPic.Action {

	public class AirPlaySelectionViewController : UITableViewController {

		static NSString cellIdentifier = new NSString ("AirPlayDeviceCell");

		static UIImage icon = UIImage.FromFile ("monitor32.png");

		string[] devices;
		NSTimer timer;

		public AirPlaySelectionViewController () : base (UITableViewStyle.Grouped)
		{
		}

		UIImage Image { get; set; }

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// close the extension (and it's resources)
			Dismiss ();
		}

		public override void ViewWillAppear (bool animated)
		{
			LoadExtensionItem ();
			timer = NSTimer.CreateRepeatingScheduledTimer (2, Refresh);
			Refresh (timer);
			base.ViewWillAppear (animated);

			Title = "Select device";
			// "CompleteRequest" -> Calling this method eventually dismisses the app extension’s view controller.
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Done, 
				(sender, e) => Dismiss ());
		}

		// Info.plist already state we only support a (single) image
		void LoadExtensionItem ()
		{
			foreach (var item in ExtensionContext.InputItems) {
				foreach (var itemProvider in item.Attachments) {
					if (!itemProvider.HasItemConformingTo (UTType.Image))
						continue;

					itemProvider.LoadItem (UTType.Image, null, delegate (NSObject image, NSError error) {
						// we can get a UIImage directly (e.g. the AirPic sample)
						Image = (image as UIImage);
						if (Image != null)
							return;

						// or we can get an NSUrl, e.g. iOS photo app
						var url = (image as NSUrl);
						if (url != null) {
							Image = UIImage.FromFile (url.Path);
							if (Image != null)
								return;
						}

						// I have not seen a comprehensive list of all types that UTType.Image can return
						// the device logs will tell you if the extension gets something it does not handle
						if (Image == null)
							Console.WriteLine ("Unsupported type: {0} {1}", image.Class.Name, image.Description);
					});
					// we only support one and there should not be more than one (Info.plist)
					return;
				}
			}
		}

		void Refresh (NSTimer timer)
		{
			devices = AirPlayBrowser.GetDeviceNames ();
			TableView.ReloadData ();
		}

		void Dismiss ()
		{
			// no need to continue browsing for devices
			AirPlayBrowser.Enabled = false;
			// Dispose
			if (timer != null) {
				timer.Invalidate ();
				timer.Dispose ();
			}
			var image = Image;
			if (Image != null)
				image.Dispose ();
			icon.Dispose ();
			// "CompleteRequest" -> Calling this method eventually dismisses the app extension’s view controller.
			ExtensionContext.CompleteRequest (ExtensionContext.InputItems, null);
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return devices == null ? 0 : devices.Length;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
				cell.ImageView.Image = icon;
			}

			var names = devices;
			try {
				var name = names [indexPath.Row];
				var device = AirPlayBrowser.GetDevice (name);
				cell.TextLabel.Text = device == null ? String.Empty : device.Name;
			}
			catch {
				// if we're unlucky
				return null;
			}
			return cell;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			// if we could not get/convert the provided data into an image
			if (Image == null)
				return;

			// just in case the device list changes in between (race) we'll try the name in the cell (not in the list)
			var cell = tableView.CellAt (indexPath);
			if (cell == null)
				return; // tableview might have refreshed

			var name = cell.TextLabel.Text;
			var device = AirPlayBrowser.GetDevice (name);
			if (device == null)
				return; // might have disappeared

			device.SendTo (Image, (complete => Dismiss ()));
		}
	}
}