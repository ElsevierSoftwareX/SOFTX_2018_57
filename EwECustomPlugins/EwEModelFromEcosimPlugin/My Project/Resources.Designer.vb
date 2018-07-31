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
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("EwEModelFromEcosimPlugin.Resources", GetType(Resources).Assembly)
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
        '''  Looks up a localized string similar to Ecopath model from Ecosim.
        '''</summary>
        Friend ReadOnly Property CONTROL_TEXT() As String
            Get
                Return ResourceManager.GetString("CONTROL_TEXT", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Create model.
        '''</summary>
        Friend ReadOnly Property HEADER_CREATE_MODEL() As String
            Get
                Return ResourceManager.GetString("HEADER_CREATE_MODEL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Ecosim year.
        '''</summary>
        Friend ReadOnly Property HEADER_ECOSIM_YEAR() As String
            Get
                Return ResourceManager.GetString("HEADER_ECOSIM_YEAR", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Model name.
        '''</summary>
        Friend ReadOnly Property HEADER_MODEL_NAME() As String
            Get
                Return ResourceManager.GetString("HEADER_MODEL_NAME", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Select output folder for new model(s).
        '''</summary>
        Friend ReadOnly Property PROMPT_OUTPUT_FOLDER() As String
            Get
                Return ResourceManager.GetString("PROMPT_OUTPUT_FOLDER", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Generating Ecopath model &apos;{0}&apos;....
        '''</summary>
        Friend ReadOnly Property STATUS_GENERATING_MODEL() As String
            Get
                Return ResourceManager.GetString("STATUS_GENERATING_MODEL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Failed to generate Ecopath model {0} for {1}.
        '''</summary>
        Friend ReadOnly Property STATUS_MODEL_FAILED() As String
            Get
                Return ResourceManager.GetString("STATUS_MODEL_FAILED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Generated Ecopath model {0}.
        '''</summary>
        Friend ReadOnly Property STATUS_MODEL_GENERATED() As String
            Get
                Return ResourceManager.GetString("STATUS_MODEL_GENERATED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Generated Ecopath models from Ecosim run.
        '''</summary>
        Friend ReadOnly Property STATUS_MODELS_GENERATED() As String
            Get
                Return ResourceManager.GetString("STATUS_MODELS_GENERATED", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Unable or not allowed to save {0}.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_ERROR_NOACCESS() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_ERROR_NOACCESS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Your system does not have the correct drivers installed to use model {0}.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_ERROR_NODRIVERS() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_ERROR_NODRIVERS", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Failed to save model {0}. See the EwE log file for details.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_ERROR_SEELOG() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_ERROR_SEELOG", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Ecosim timestep {0} saved to {1}.
        '''</summary>
        Friend ReadOnly Property STATUS_SAVE_SUCCESS() As String
            Get
                Return ResourceManager.GetString("STATUS_SAVE_SUCCESS", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
