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
using UIKit;

using Poupou.AirPlay;

namespace AirPic.Action {

	// note: we're not using a storyboard for the extension (see Info.plist)

	[Register ("ActionViewController")]
	public class ActionViewController : UINavigationController {

		public ActionViewController (IntPtr handle) : base (handle)
		{
			// enable browsing asap... as it might take a bit of time to get the devices from the network
			AirPlayBrowser.Enabled = true;
		}

		public override void ViewDidLoad ()
		{
			SetViewControllers (new UIViewController [] { new AirPlaySelectionViewController () }, true);
			base.ViewDidLoad ();
		}
	}
}