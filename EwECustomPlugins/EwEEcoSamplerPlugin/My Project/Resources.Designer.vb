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
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("EwEEcoSamplerPlugin.Resources", GetType(Resources).Assembly)
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
        '''  Looks up a localized string similar to Date.
        '''</summary>
        Friend ReadOnly Property HEADER_DATE() As String
            Get
                Return ResourceManager.GetString("HEADER_DATE", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Loaded.
        '''</summary>
        Friend ReadOnly Property HEADER_LOADED() As String
            Get
                Return ResourceManager.GetString("HEADER_LOADED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to No. EE&gt;1.
        '''</summary>
        Friend ReadOnly Property HEADER_NUM_INVALID_EE() As String
            Get
                Return ResourceManager.GetString("HEADER_NUM_INVALID_EE", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Perturbed.
        '''</summary>
        Friend ReadOnly Property HEADER_PERTURBED() As String
            Get
                Return ResourceManager.GetString("HEADER_PERTURBED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Rating.
        '''</summary>
        Friend ReadOnly Property HEADER_RATING() As String
            Get
                Return ResourceManager.GetString("HEADER_RATING", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to System.
        '''</summary>
        Friend ReadOnly Property HEADER_SYSTEM() As String
            Get
                Return ResourceManager.GetString("HEADER_SYSTEM", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &amp;Load.
        '''</summary>
        Friend ReadOnly Property LABEL_LOAD() As String
            Get
                Return ResourceManager.GetString("LABEL_LOAD", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &amp;Unload.
        '''</summary>
        Friend ReadOnly Property LABEL_UNLOAD() As String
            Get
                Return ResourceManager.GetString("LABEL_UNLOAD", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Record balanced models from Monte Carlo.
        '''</summary>
        Friend ReadOnly Property MENUITEM_TEXT() As String
            Get
                Return ResourceManager.GetString("MENUITEM_TEXT", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to You are about to run {0} samples through EwE. Proceed if the desired Ecosim and Ecospace scenarios are loaded, desired plug-ins are activated, and desired outputs are properly configured to save results to disk. Last, the drive for receiving EwE output data should have ample free space.
        '''
        '''Do you wish to start the batch run?.
        '''</summary>
        Friend ReadOnly Property PROMPT_BATCHRUN() As String
            Get
                Return ResourceManager.GetString("PROMPT_BATCHRUN", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Select EwE model file to import samples from.
        '''</summary>
        Friend ReadOnly Property PROMPT_IMPORT_MODEL() As String
            Get
                Return ResourceManager.GetString("PROMPT_IMPORT_MODEL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Failed to record Monte Carlo balanced model, recording has stopped. Please see the error log for details..
        '''</summary>
        Friend ReadOnly Property RECORD_ERROR() As String
            Get
                Return ResourceManager.GetString("RECORD_ERROR", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} Monte Carlo samples recorded.
        '''</summary>
        Friend ReadOnly Property RECORD_REPORT() As String
            Get
                Return ResourceManager.GetString("RECORD_REPORT", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property RecordHS() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("RecordHS", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property RecordingHS() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("RecordingHS", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property SampleHS() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("SampleHS", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Ecosampler.
        '''</summary>
        Friend ReadOnly Property TABTEXT() As String
            Get
                Return ResourceManager.GetString("TABTEXT", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to (this computer).
        '''</summary>
        Friend ReadOnly Property VALUE_THISCOMPUTER() As String
            Get
                Return ResourceManager.GetString("VALUE_THISCOMPUTER", resourceCulture)
            End Get
        End Property
    End Module
End Namespace