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
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("EwEStepwiseFittingPlugin.Resources", GetType(Resources).Assembly)
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
        '''  Looks up a localized string similar to Stepwise fitting.
        '''</summary>
        Friend ReadOnly Property CAPTION() As String
            Get
                Return ResourceManager.GetString("CAPTION", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to An engine that provides a systematic exploration of time series fitting options.
        '''</summary>
        Friend ReadOnly Property DESCRIPTION() As String
            Get
                Return ResourceManager.GetString("DESCRIPTION", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Ecosim.
        '''</summary>
        Friend ReadOnly Property DETAIL_ECOSIM() As String
            Get
                Return ResourceManager.GetString("DETAIL_ECOSIM", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Aggregated iteration results.
        '''</summary>
        Friend ReadOnly Property DETAIL_ITERATION_AGGREGATED() As String
            Get
                Return ResourceManager.GetString("DETAIL_ITERATION_AGGREGATED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Iteration configuration.
        '''</summary>
        Friend ReadOnly Property DETAIL_ITERATION_CONFIG() As String
            Get
                Return ResourceManager.GetString("DETAIL_ITERATION_CONFIG", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Summary.
        '''</summary>
        Friend ReadOnly Property DETAIL_SUMMARY() As String
            Get
                Return ResourceManager.GetString("DETAIL_SUMMARY", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cannot access directory {0}..
        '''</summary>
        Friend ReadOnly Property FAILURE_DIRECTORY() As String
            Get
                Return ResourceManager.GetString("FAILURE_DIRECTORY", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} running {1}....
        '''</summary>
        Friend ReadOnly Property STATUS_RUNNING() As String
            Get
                Return ResourceManager.GetString("STATUS_RUNNING", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} results could not be saved. {1}.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_DETAIL_FAILED() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_DETAIL_FAILED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} results saved to {1}.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_DETAIL_SUCCESS() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_DETAIL_SUCCESS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} results could not be saved.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_FAILED() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_FAILED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} results saved successfully.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_SUCCESS() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_SUCCESS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} saving CSV files....
        '''</summary>
        Friend ReadOnly Property STATUS_SAVING() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVING", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
