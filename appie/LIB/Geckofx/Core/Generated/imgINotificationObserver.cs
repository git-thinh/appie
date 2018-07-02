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
// Generated by IDLImporter from file imgINotificationObserver.idl
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
    ///-*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*-
    ///
    /// This Source Code Form is subject to the terms of the Mozilla Public
    /// License, v. 2.0. If a copy of the MPL was not distributed with this
    /// file, You can obtain one at http://mozilla.org/MPL/2.0/. </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("ac65c702-7771-4f6d-b18b-1c7d806ce3c1")]
	public interface imgINotificationObserver
	{
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void Notify(imgIRequest aProxy, int aType, [MarshalAs(UnmanagedType.Interface)] nsIntRect aRect);
	}
	
	/// <summary>imgINotificationObserverConsts </summary>
	public class imgINotificationObserverConsts
	{
		
		// <summary>
        //-*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*-
        //
        // This Source Code Form is subject to the terms of the Mozilla Public
        // License, v. 2.0. If a copy of the MPL was not distributed with this
        // file, You can obtain one at http://mozilla.org/MPL/2.0/. </summary>
		public const long SIZE_AVAILABLE = 1;
		
		// 
		public const long FRAME_UPDATE = 2;
		
		// 
		public const long FRAME_COMPLETE = 3;
		
		// 
		public const long LOAD_COMPLETE = 4;
		
		// 
		public const long DECODE_COMPLETE = 5;
		
		// 
		public const long DISCARD = 6;
		
		// 
		public const long UNLOCKED_DRAW = 7;
		
		// 
		public const long IS_ANIMATED = 8;
	}
}
