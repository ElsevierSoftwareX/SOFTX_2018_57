﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("EwEAquamapsEnvDataImporterPlugin.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property aquamaps_jpg() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("aquamaps_jpg", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Select Aquamaps HSPEN files to load.
        '''</summary>
        Friend ReadOnly Property CAPTION_LOAD_HSPEN() As String
            Get
                Return ResourceManager.GetString("CAPTION_LOAD_HSPEN", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Mapping parameters for Sardina pilchardus (European pilchard),,,,,
        ''',,,,,
        '''Map type: Expert-reviewed,,,,,
        '''Map Option: In FAO and Bounding Box,,,,,
        '''FAOAreas:  1 |  4 |  5 |  27 |  34 |  37  |  37 ,,,,,
        '''Bounding Box (NSWE): ,68,14,-32,43,
        ''',,,,,
        '''Pelagic: False,,,,,
        '''Layer used to generate probabilities: Surface,,,,,
        ''',,,,,
        '''Species Envelope (HSPEN):,,,,,
        ''' ,Used,Min,Pref Min (10th),Pref Max (90th),Max
        '''Depth (m),1,10,25,100,100
        '''Temperature (°C),1,2.53,12,26.92,27.62
        '''Salinity (psu),1,18.1,34.72,37.99,39. [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property HSPEN_example_csv() As String
            Get
                Return ResourceManager.GetString("HSPEN_example_csv", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property jrc_logo() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("jrc_logo", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Aquamaps CSV file {0} is malformed, cannot continue.
        '''</summary>
        Friend ReadOnly Property PROMPT_FILE_INVALID() As String
            Get
                Return ResourceManager.GetString("PROMPT_FILE_INVALID", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to AquaMaps CSV file {0} did not contain any valid distribution envelopes.
        '''</summary>
        Friend ReadOnly Property PROMPT_FILE_NOCONTENT() As String
            Get
                Return ResourceManager.GetString("PROMPT_FILE_NOCONTENT", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Unable to load Aquamaps distribution envelopes. {0}.
        '''</summary>
        Friend ReadOnly Property PROMPT_FILE_NOT_FOUND() As String
            Get
                Return ResourceManager.GetString("PROMPT_FILE_NOT_FOUND", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Failed to add response function &apos;{0}&apos;.
        '''</summary>
        Friend ReadOnly Property PROMPT_IMPORT_DETAIL_FAILED() As String
            Get
                Return ResourceManager.GetString("PROMPT_IMPORT_DETAIL_FAILED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cannot interpret &apos;{0}&apos;.
        '''</summary>
        Friend ReadOnly Property PROMPT_IMPORT_DETAIL_LINEERROR() As String
            Get
                Return ResourceManager.GetString("PROMPT_IMPORT_DETAIL_LINEERROR", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Added response function &apos;{0}&apos;.
        '''</summary>
        Friend ReadOnly Property PROMPT_IMPORT_DETAIL_SUCCESS() As String
            Get
                Return ResourceManager.GetString("PROMPT_IMPORT_DETAIL_SUCCESS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Imported Aquamaps species distribution envelopes.
        '''</summary>
        Friend ReadOnly Property PROMPT_IMPORT_GENERIC() As String
            Get
                Return ResourceManager.GetString("PROMPT_IMPORT_GENERIC", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Configuring functions.
        '''</summary>
        Friend ReadOnly Property STATUS_CONFIGURING() As String
            Get
                Return ResourceManager.GetString("STATUS_CONFIGURING", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Creating response functions.
        '''</summary>
        Friend ReadOnly Property STATUS_CREATING() As String
            Get
                Return ResourceManager.GetString("STATUS_CREATING", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Creating response function &apos;{0}&apos;.
        '''</summary>
        Friend ReadOnly Property STATUS_CREATING_DETAIL() As String
            Get
                Return ResourceManager.GetString("STATUS_CREATING_DETAIL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Example Aquamaps HSPEN file failed to save {0}.
        '''</summary>
        Friend ReadOnly Property STATUS_EXAMPLE_SAVE_FAILED() As String
            Get
                Return ResourceManager.GetString("STATUS_EXAMPLE_SAVE_FAILED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Example Aquamaps HSPEN file saved to {0}.
        '''</summary>
        Friend ReadOnly Property STATUS_EXAMPLE_SAVE_SUCCESS() As String
            Get
                Return ResourceManager.GetString("STATUS_EXAMPLE_SAVE_SUCCESS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Importing aquamaps species envelopes.
        '''</summary>
        Friend ReadOnly Property STATUS_IMPORTING() As String
            Get
                Return ResourceManager.GetString("STATUS_IMPORTING", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Saving changes.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVING() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVING", resourceCulture)
            End Get
        End Property
    End Module
End Namespace