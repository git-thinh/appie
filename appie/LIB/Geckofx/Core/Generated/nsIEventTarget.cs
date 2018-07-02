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
// Generated by IDLImporter from file nsIEventTarget.idl
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
	[Guid("4e8febe4-6631-49dc-8ac9-308c1cb9b09c")]
	public interface nsIEventTarget
	{
		
		/// <summary>
        /// Dispatch an event to this event target.  This function may be called from
        /// any thread, and it may be called re-entrantly.
        ///
        /// @param event
        /// The event to dispatch.
        /// @param flags
        /// The flags modifying event dispatch.  The flags are described in detail
        /// below.
        ///
        /// @throws NS_ERROR_INVALID_ARG
        /// Indicates that event is null.
        /// @throws NS_ERROR_UNEXPECTED
        /// Indicates that the thread is shutting down and has finished processing
        /// events, so this event would never run and has not been dispatched.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void Dispatch([MarshalAs(UnmanagedType.Interface)] nsIRunnable @event, uint flags);
		
		/// <summary>
        /// Check to see if this event target is associated with the current thread.
        ///
        /// @returns
        /// A boolean value that if "true" indicates that events dispatched to this
        /// event target will run on the current thread (i.e., the thread calling
        /// this method).
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool IsOnCurrentThread();
	}
	
	/// <summary>nsIEventTargetConsts </summary>
	public class nsIEventTargetConsts
	{
		
		// <summary>
        // This flag specifies the default mode of event dispatch, whereby the event
        // is simply queued for later processing.  When this flag is specified,
        // dispatch returns immediately after the event is queued.
        // </summary>
		public const ulong DISPATCH_NORMAL = 0;
		
		// <summary>
        // This flag specifies the synchronous mode of event dispatch, in which the
        // dispatch method does not return until the event has been processed.
        //
        // NOTE: passing this flag to dispatch may have the side-effect of causing
        // other events on the current thread to be processed while waiting for the
        // given event to be processed.
        // </summary>
		public const ulong DISPATCH_SYNC = 1;
	}
}
