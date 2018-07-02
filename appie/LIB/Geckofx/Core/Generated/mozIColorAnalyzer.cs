// --------------------------------------------------------------------------------------------
// Version: MPL 1.1/GPL 2.0/LGPL 2.1
// 
// The contents of this file are subject to the Mozilla Public License Version
// 1.1 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
// for the specific language governing rights and limitations under the
// License.
// 
// <remarks>
// Generated by IDLImporter from file mozIColorAnalyzer.idl
// 
// You should use these interfaces when you access the COM objects defined in the mentioned
// IDL/IDH file.
// </remarks>
// --------------------------------------------------------------------------------------------
namespace Gecko
{
	using System;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Runtime.CompilerServices;
	
	
	/// <summary>
    ///This Source Code Form is subject to the terms of the Mozilla Public
    /// License, v. 2.0. If a copy of the MPL was not distributed with this
    /// file, You can obtain one at http://mozilla.org/MPL/2.0/. </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("e4089e21-71b6-40af-b546-33c21b90e874")]
	public interface mozIRepresentativeColorCallback
	{
		
		/// <summary>
        /// Will be called when color analysis finishes.
        ///
        /// @param success
        /// True if analysis was successful, false otherwise.
        /// Analysis can fail if the image is transparent, imageURI doesn't
        /// resolve to a valid image, or the image is too big.
        ///
        /// @param color
        /// The representative color as an integer in RGB form.
        /// e.g. 0xFF0102 == rgb(255,1,2)
        /// If success is false, color is not provided.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void OnComplete([MarshalAs(UnmanagedType.U1)] bool success, uint color);
	}
	
	/// <summary>mozIColorAnalyzer </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("d056186c-28a0-494e-aacc-9e433772b143")]
	public interface mozIColorAnalyzer
	{
		
		/// <summary>
        /// Given an image URI, find the most representative color for that image
        /// based on the frequency of each color.  Preference is given to colors that
        /// are more interesting.  Avoids the background color if it can be
        /// discerned.  Ignores sufficiently transparent colors.
        ///
        /// This is intended to be used on favicon images.  Larger images take longer
        /// to process, especially those with a larger number of unique colors.  If
        /// imageURI points to an image that has more than 128^2 pixels, this method
        /// will fail before analyzing it for performance reasons.
        ///
        /// @param imageURI
        /// A URI pointing to the image - ideally a data: URI, but any scheme
        /// that will load when setting the src attribute of a DOM img element
        /// should work.
        /// @param callback
        /// Function to call when the representative color is found or an
        /// error occurs.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void FindRepresentativeColor([MarshalAs(UnmanagedType.Interface)] nsIURI imageURI, mozIRepresentativeColorCallback callback);
	}
}
