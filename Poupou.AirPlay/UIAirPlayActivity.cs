// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
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
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Poupou.AirPlay {

	/// <summary>
	/// This is a UIActivity subclass that allows sending an UIImage to a specific AirPlay device.
	/// You only need to use the static GetCurrentActivities method to get UIActivity for each AirPlay device visible
	/// on the network. Everything else will be handled automagically.
	/// You might want ot customize/override the Image property to provide your own icon or change the Title property
	/// to show something else than the AirPlay device's name.
	/// Note: UIActivity is only available on iOS6 and later.
	/// </summary>
	public class UIAirPlayActivity : UIActivity {
			
		static UIImage logo;
		static NSString type;
		NSNetService device;
		UIImage image;

		public UIAirPlayActivity (NSNetService service)
		{
			device = service;
		}
		
		public override NSString Type {
			get {
				if (type == null)
					type = new NSString ("UIAirPlayActivity");
				return type;
			}
		}
		
		public override string Title {
			get { return device.Name ?? "AirPlay"; }
		}
		
		public override UIImage Image {
			get {
				if (logo == null)
					logo = GetIcon ();
				return logo;
			}
		}
		
		// This activity only support one UIImage
		public override bool CanPerform (NSObject[] activityItems)
		{
			if (activityItems.Length != 1)
				return false;
			return (activityItems [0] is UIImage);
		}
		
		// Hold a reference to the UIImage we'll perform on
		public override void Prepare (NSObject[] activityItems)
		{
			image = activityItems [0] as UIImage;
		}
		
		public override void Perform ()
		{
			try {
				device.SendTo (image, Finished);
			}
			finally {
				image = null; // drop reference
			}
		}

		/// <summary>
		/// Retrieve a UIActivity instance for each AirPlay device currently accessible on the network.
		/// </summary>
		/// <returns>
		/// An array of UIActivity that can be supplied to UIActivityViewController constructor
		/// </returns>
		public static UIActivity[] GetCurrentActivities ()
		{
			List<UIActivity> list = new List<UIActivity> ();
			foreach (var device in AirPlayBrowser.Devices) {
				list.Add (new UIAirPlayActivity (device));
			}
			return list.ToArray ();
		}

		// simple icon showing "tv", override Image property to show your own image
		static UIImage GetIcon ()
		{
			// http://developer.apple.com/library/ios/#documentation/UIKit/Reference/UIActivity_Class/Reference/Reference.html
			// iPhone / iPod Touch: 43x43 (86x86 retina)
			// iPad: 55x55 (110x110 retina)
			float size = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? 55f : 43f;
			// http://tirania.org/blog/archive/2010/Jul-20-2.html
			UIGraphics.BeginImageContextWithOptions (new SizeF (size, size), false, 0.0f);
			using (var c = UIGraphics.GetCurrentContext ()) {
				c.SetFillColor (1.0f, 1.0f, 1.0f, 1.0f);
				c.SetStrokeColor (1.0f, 1.0f, 1.0f, 1.0f);
				UIFont font = UIFont.BoldSystemFontOfSize (size - 8);
				using (var s = new NSString ("tv"))
					s.DrawString (new PointF (7.5f, 0.0f), font);
			}
			UIImage img = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return img;
		}
	}
}