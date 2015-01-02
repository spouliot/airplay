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
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;

using Foundation;
using UIKit;

namespace Poupou.AirPlay {

	/// <summary>
	/// AirPlay service browser.
	/// </summary>
	public static class AirPlayBrowser	{

		static NSNetServiceBrowser browser;
		static ConcurrentDictionary<string, NSNetService> services;
		static bool enabled;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Poupou.AirPlay.AirPlayBrowser"/> is enabled.
		/// </summary>
		/// <value>
		/// Set to <c>true</c> to enable AirPlay device monitoring; otherwise, <c>false</c> (default).
		/// </value>
		static public bool Enabled {
			get { return enabled; }
			set {
				if (value == enabled)
					return;
				
				if (value)
					Enable ();
				else
					Disable ();
			}
		}

		/// <summary>
		/// Iterate thru all the AirPlay devices currently accessible.
		/// </summary>
		/// <value>
		/// The AirPlay devices.
		/// </value>
		static public IEnumerable<NSNetService> Devices { 
			get {
				if (services == null)
					yield break;
				foreach (var device in services.Values)
					yield return device;
			}
		}

		/// <summary>
		/// Gets the NSNetService associated with the specified name.
		/// </summary>
		/// <returns>
		/// The AirPlay device or null if the device does not exists or is not available anymore.
		/// </returns>
		/// <param name='name'>
		/// The AirPlay device name.
		/// </param>
		static public NSNetService GetDevice (string name)
		{
			if (services == null)
				return null;
			NSNetService service = null;
			services.TryGetValue (name, out service);
			return service;
		}

		/// <summary>
		/// Gets all the presently available AirPlay device names.
		/// </summary>
		/// <returns>
		/// A string array of all device names.
		/// </returns>
		static public string[] GetDeviceNames ()
		{
			if (services == null)
				return new string[0];
			return services.Keys.ToArray ();
		}

		static void Disable ()
		{
			enabled = false;
			if (browser != null)
				browser.Stop ();
		}

		static void Enable ()
		{
			enabled = true;
			if (browser == null) {
				browser = new NSNetServiceBrowser ();
				browser.FoundService += delegate (object sender, NSNetServiceEventArgs e) {
					if (services == null)
						services = new ConcurrentDictionary<string, NSNetService> ();
					var service = e.Service;
#if DEBUG
					service.AddressResolved += (sender2, e2) => {
						var s = sender2 as NSNetService;
						Debug.WriteLine ("{0} {1}:{2}", s.Name, s.HostName, s.Port);
					};
#endif
					service.Resolve (2.0d);
					services.TryAdd (service.Name, service);
				};
				browser.ServiceRemoved += delegate (object sender, NSNetServiceEventArgs e) {
					string name = e.Service.Name;
					if (!String.IsNullOrEmpty (name)) {
						NSNetService service;
						services.TryRemove (name, out service);
					}
				};
			}
			browser.SearchForServices ("_airplay._tcp", "local.");
		}
	}
}