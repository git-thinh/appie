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
// Generated by IDLImporter from file nsIBrowserSearchService.idl
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
	[Guid("5799251f-5b55-4df7-a9e7-0c27812c469a")]
	public interface nsISearchSubmission
	{
		
		/// <summary>
        /// The POST data associated with a search submission, wrapped in a MIME
        /// input stream. May be null.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsIInputStream GetPostDataAttribute();
		
		/// <summary>
        /// The URI to submit a search to.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsIURI GetUriAttribute();
	}
	
	/// <summary>nsISearchEngine </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("65fd517b-6bda-4bac-bbe1-42de1b3a7df6")]
	public interface nsISearchEngine
	{
		
		/// <summary>
        /// Gets a nsISearchSubmission object that contains information about what to
        /// send to the search engine, including the URI and postData, if applicable.
        ///
        /// @param  data
        /// Data to add to the submission object.
        /// i.e. the search terms.
        ///
        /// @param  responseType [optional]
        /// The MIME type that we'd like to receive in response
        /// to this submission.  If null, will default to "text/html".
        ///
        /// @param purpose [optional]
        /// A string meant to indicate the context of the search request. This
        /// allows the search service to provide a different nsISearchSubmission
        /// depending on e.g. where the search is triggered in the UI.
        ///
        /// @returns A nsISearchSubmission object that contains information about what
        /// to send to the search engine.  If no submission can be
        /// obtained for the given responseType, returns null.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsISearchSubmission GetSubmission([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase data, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase responseType, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase purpose);
		
		/// <summary>
        /// Adds a parameter to the search engine's submission data. This should only
        /// be called on engines created via addEngineWithDetails.
        ///
        /// @param name
        /// The parameter's name. Must not be null.
        ///
        /// @param value
        /// The value to pass. If value is "{searchTerms}", it will be
        /// substituted with the user-entered data when retrieving the
        /// submission. Must not be null.
        ///
        /// @param responseType
        /// Since an engine can have several different request URLs,
        /// differentiated by response types, this parameter selects
        /// a request to add parameters to.  If null, will default
        /// to "text/html"
        ///
        /// @throws NS_ERROR_FAILURE if the search engine is read-only.
        /// @throws NS_ERROR_INVALID_ARG if name or value are null.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void AddParam([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase name, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase value, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase responseType);
		
		/// <summary>
        /// Determines whether the engine can return responses in the given
        /// MIME type.  Returns true if the engine spec has a URL with the
        /// given responseType, false otherwise.
        ///
        /// @param responseType
        /// The MIME type to check for
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool SupportsResponseType([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase responseType);
		
		/// <summary>
        /// Returns a string with the URL to an engine's icon matching both width and
        /// height. Returns null if icon with specified dimensions is not found.
        ///
        /// @param width
        /// Width of the requested icon.
        /// @param height
        /// Height of the requested icon.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetIconURLBySize(int width, int height, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase retval);
		
		/// <summary>
        /// Gets an array of all available icons. Each entry is an object with
        /// width, height and url properties. width and height are numeric and
        /// represent the icon's dimensions. url is a string with the URL for
        /// the icon.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		Gecko.JsVal GetIcons();
		
		/// <summary>
        /// Opens a speculative connection to the engine's search URI
        /// (and suggest URI, if different) to reduce request latency
        ///
        /// @param  options
        /// An object that must contain the following fields:
        /// {window} the content window for the window performing the search
        ///
        /// @throws NS_ERROR_INVALID_ARG if options is omitted or lacks required
        /// elemeents
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void SpeculativeConnect(ref Gecko.JsVal options);
		
		/// <summary>
        /// An optional shortcut alias for the engine.
        /// When non-null, this is a unique identifier.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetAliasAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aAlias);
		
		/// <summary>
        /// An optional shortcut alias for the engine.
        /// When non-null, this is a unique identifier.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void SetAliasAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aAlias);
		
		/// <summary>
        /// A text description describing the engine.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetDescriptionAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aDescription);
		
		/// <summary>
        /// Whether the engine should be hidden from the user.
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool GetHiddenAttribute();
		
		/// <summary>
        /// Whether the engine should be hidden from the user.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void SetHiddenAttribute([MarshalAs(UnmanagedType.U1)] bool aHidden);
		
		/// <summary>
        /// A nsIURI corresponding to the engine's icon, stored locally. May be null.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsIURI GetIconURIAttribute();
		
		/// <summary>
        /// The display name of the search engine. This is a unique identifier.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetNameAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aName);
		
		/// <summary>
        /// A URL string pointing to the engine's search form.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetSearchFormAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aSearchForm);
		
		/// <summary>
        /// The search engine type.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		int GetTypeAttribute();
		
		/// <summary>
        /// An optional unique identifier for this search engine within the context of
        /// the distribution, as provided by the distributing entity.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetIdentifierAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aIdentifier);
		
		/// <summary>
        /// Gets a string representing the hostname from which search results for a
        /// given responseType are returned, minus the leading "www." (if present).
        /// This can be specified as an url attribute in the engine description file,
        /// but will default to host from the <Url>'s template otherwise.
        ///
        /// @param  responseType [optional]
        /// The MIME type to get resultDomain for.  Defaults to "text/html".
        ///
        /// @return the resultDomain for the given responseType.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetResultDomain([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase responseType, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase retval);
	}
	
	/// <summary>nsISearchEngineConsts </summary>
	public class nsISearchEngineConsts
	{
		
		// <summary>
        // Supported search engine types.
        // </summary>
		public const ulong TYPE_MOZSEARCH = 1;
		
		// 
		public const ulong TYPE_SHERLOCK = 2;
		
		// 
		public const ulong TYPE_OPENSEARCH = 3;
		
		// <summary>
        // Supported search engine data types.
        // </summary>
		public const ulong DATA_XML = 1;
		
		// 
		public const ulong DATA_TEXT = 2;
	}
	
	/// <summary>nsISearchInstallCallback </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("9fc39136-f08b-46d3-b232-96f4b7b0e235")]
	public interface nsISearchInstallCallback
	{
		
		/// <summary>
        /// Called to indicate that the engine addition process succeeded.
        ///
        /// @param engine
        /// The nsISearchEngine object that was added (will not be null).
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void OnSuccess([MarshalAs(UnmanagedType.Interface)] nsISearchEngine engine);
		
		/// <summary>
        /// Called to indicate that the engine addition process failed.
        ///
        /// @param errorCode
        /// One of the ERROR_* values described above indicating the cause of
        /// the failure.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void OnError(uint errorCode);
	}
	
	/// <summary>nsISearchInstallCallbackConsts </summary>
	public class nsISearchInstallCallbackConsts
	{
		
		// 
		public const ulong ERROR_UNKNOWN_FAILURE = 0x1;
		
		// 
		public const ulong ERROR_DUPLICATE_ENGINE = 0x2;
	}
	
	/// <summary>
    /// Callback for asynchronous initialization of nsIBrowserSearchService
    /// </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("02256156-16e4-47f1-9979-76ff98ceb590")]
	public interface nsIBrowserSearchInitObserver
	{
		
		/// <summary>
        /// Called once initialization of the browser search service is complete.
        ///
        /// @param aStatus The status of that service.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void OnInitComplete(int aStatus);
	}
	
	/// <summary>nsIBrowserSearchService </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("939d74a4-5b01-463c-80c7-4301f0c0f9ef")]
	public interface nsIBrowserSearchService
	{
		
		/// <summary>
        /// Start asynchronous initialization.
        ///
        /// The callback is triggered once initialization is complete, which may be
        /// immediately, if initialization has already been completed by some previous
        /// call to this method. The callback is always invoked asynchronously.
        ///
        /// @param aObserver An optional object observing the end of initialization.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void Init([MarshalAs(UnmanagedType.Interface)] nsIBrowserSearchInitObserver aObserver);
		
		/// <summary>
        /// Determine whether initialization has been completed.
        ///
        /// Clients of the service can use this attribute to quickly determine whether
        /// initialization is complete, and decide to trigger some immediate treatment,
        /// to launch asynchronous initialization or to bailout.
        ///
        /// Note that this attribute does not indicate that initialization has succeeded.
        ///
        /// @return |true| if the search service is now initialized, |false| if
        /// initialization has not been triggered yet.
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool GetIsInitializedAttribute();
		
		/// <summary>
        /// Adds a new search engine from the file at the supplied URI, optionally
        /// asking the user for confirmation first.  If a confirmation dialog is
        /// shown, it will offer the option to begin using the newly added engine
        /// right away.
        ///
        /// @param engineURL
        /// The URL to the search engine's description file.
        ///
        /// @param dataType
        /// An integer representing the plugin file format. Must be one
        /// of the supported search engine data types defined above.
        ///
        /// @param iconURL
        /// A URL string to an icon file to be used as the search engine's
        /// icon. This value may be overridden by an icon specified in the
        /// engine description file.
        ///
        /// @param confirm
        /// A boolean value indicating whether the user should be asked for
        /// confirmation before this engine is added to the list.  If this
        /// value is false, the engine will be added to the list upon successful
        /// load, but it will not be selected as the current engine.
        ///
        /// @param callback
        /// A nsISearchInstallCallback that will be notified when the
        /// addition is complete, or if the addition fails. It will not be
        /// called if addEngine throws an exception.
        ///
        /// @throws NS_ERROR_FAILURE if the type is invalid, or if the description
        /// file cannot be successfully loaded.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void AddEngine([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase engineURL, int dataType, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase iconURL, [MarshalAs(UnmanagedType.U1)] bool confirm, [MarshalAs(UnmanagedType.Interface)] nsISearchInstallCallback callback);
		
		/// <summary>
        /// Adds a new search engine, without asking the user for confirmation and
        /// without starting to use it right away.
        ///
        /// @param name
        /// The search engine's name. Must be unique. Must not be null.
        ///
        /// @param iconURL
        /// Optional: A URL string pointing to the icon to be used to represent
        /// the engine.
        ///
        /// @param alias
        /// Optional: A unique shortcut that can be used to retrieve the
        /// search engine.
        ///
        /// @param description
        /// Optional: a description of the search engine.
        ///
        /// @param method
        /// The HTTP request method used when submitting a search query.
        /// Must be a case insensitive value of either "get" or "post".
        ///
        /// @param url
        /// The URL to which search queries should be sent.
        /// Must not be null.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void AddEngineWithDetails([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase name, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase iconURL, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase alias, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase description, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase method, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase url);
		
		/// <summary>
        /// Un-hides all engines installed in the directory corresponding to
        /// the directory service's NS_APP_SEARCH_DIR key. (i.e. the set of
        /// engines returned by getDefaultEngines)
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void RestoreDefaultEngines();
		
		/// <summary>
        /// Returns an engine with the specified alias.
        ///
        /// @param   alias
        /// The search engine's alias.
        /// @returns The corresponding nsISearchEngine object, or null if it doesn't
        /// exist.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsISearchEngine GetEngineByAlias([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase alias);
		
		/// <summary>
        /// Returns an engine with the specified name.
        ///
        /// @param   aEngineName
        /// The name of the engine.
        /// @returns The corresponding nsISearchEngine object, or null if it doesn't
        /// exist.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsISearchEngine GetEngineByName([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aEngineName);
		
		/// <summary>
        /// Returns an array of all installed search engines.
        ///
        /// @returns an array of nsISearchEngine objects.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetEngines(ref uint engineCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ref nsISearchEngine[] engines);
		
		/// <summary>
        /// Returns an array of all installed search engines whose hidden attribute is
        /// false.
        ///
        /// @returns an array of nsISearchEngine objects.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetVisibleEngines(ref uint engineCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ref nsISearchEngine[] engines);
		
		/// <summary>
        /// Returns an array of all default search engines. This includes all loaded
        /// engines that aren't in the user's profile directory
        /// (NS_APP_USER_SEARCH_DIR).
        ///
        /// @returns an array of nsISearchEngine objects.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetDefaultEngines(ref uint engineCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ref nsISearchEngine[] engines);
		
		/// <summary>
        /// Moves a visible search engine.
        ///
        /// @param  engine
        /// The engine to move.
        /// @param  newIndex
        /// The engine's new index in the set of visible engines.
        ///
        /// @throws NS_ERROR_FAILURE if newIndex is out of bounds, or if engine is
        /// hidden.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void MoveEngine([MarshalAs(UnmanagedType.Interface)] nsISearchEngine engine, int newIndex);
		
		/// <summary>
        /// Removes the search engine. If the search engine is installed in a global
        /// location, this will just hide the engine. If the engine is in the user's
        /// profile directory, it will be removed from disk.
        ///
        /// @param  engine
        /// The engine to remove.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void RemoveEngine([MarshalAs(UnmanagedType.Interface)] nsISearchEngine engine);
		
		/// <summary>
        /// The default search engine. Returns the first visible engine if the default
        /// engine is hidden. May be null if there are no visible search engines.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsISearchEngine GetDefaultEngineAttribute();
		
		/// <summary>
        /// The default search engine. Returns the first visible engine if the default
        /// engine is hidden. May be null if there are no visible search engines.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void SetDefaultEngineAttribute([MarshalAs(UnmanagedType.Interface)] nsISearchEngine aDefaultEngine);
		
		/// <summary>
        /// The currently active search engine. May be null if there are no visible
        /// search engines.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsISearchEngine GetCurrentEngineAttribute();
		
		/// <summary>
        /// The currently active search engine. May be null if there are no visible
        /// search engines.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void SetCurrentEngineAttribute([MarshalAs(UnmanagedType.Interface)] nsISearchEngine aCurrentEngine);
	}
}
