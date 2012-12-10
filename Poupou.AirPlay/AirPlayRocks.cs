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
// limitations under the License.

using System;
using System.IO;
using System.Net;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Poupou.AirPlay {

	/// <summary>
	/// Extension methods for AirPlay support.
	/// </summary>
	public static class AirPlayRocks {

		static Guid session = Guid.NewGuid ();

		/// <summary>
		/// Sends the specified UIImage to the AirPlay device.
		/// </summary>
		/// <param name='service'>
		/// NSNetService (extension method target) representing the AirPlay device.
		/// </param>
		/// <param name='image'>
		/// The UIImage to be send to the device.
		/// </param>
		/// <param name='complete'>
		/// Optional method to be called when the operation is complete. True will be supplied if the action was
		/// successful, false if a problem occured.
		/// </param>
		static unsafe public void SendTo (this NSNetService service, UIImage image, Action<bool> complete)
		{
			if (service == null) {
				if (complete != null)
					complete (false);
				return;
			}
			
			// In general I prefer WebClient *Async methods but it does not provide methods to
			// upload Stream and allocating a (not really required) byte[] is a huge waste
			ThreadPool.QueueUserWorkItem (delegate {
				bool ok = true;
				try {
					string url = String.Format ("http://{0}:{1}/photo", service.HostName, service.Port);
					HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
					using (var data = image.AsJPEG ()) {
						req.Method = "PUT";
						req.ContentLength = data.Length;
						req.UserAgent = "AirPlay/160.4 (Photos)";
						req.Headers.Add ("X-Apple-AssetKey", Guid.NewGuid ().ToString ());
						req.Headers.Add ("X-Apple-Session-ID", session.ToString ());
						var s = req.GetRequestStream ();
						using (Stream ums = new UnmanagedMemoryStream ((byte *) data.Bytes, data.Length))
							ums.CopyTo (s);
					}
					req.GetResponse ().Dispose ();
				}
				catch {
					ok = false;
				}
				finally {
					if (complete != null) {
						NSRunLoop.Main.InvokeOnMainThread (delegate {
							complete (ok);
						});
					}
				}
			});
		}
	}
}