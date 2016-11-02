﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Koden er generert av et verktøy.
//     Kjøretidsversjon:4.0.30319.42000
//
//     Endringer i denne filen kan føre til feil virkemåte, og vil gå tapt hvis
//     koden genereres på nytt.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 
namespace Geonorge.NedlastingApi.Models {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Area", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class AreaType {
        
        private string typeField;
        
        private string nameField;
        
        private string codeField;
        
        private ProjectionType[] projectionsField;
        
        private FormatType[] formatsField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string code {
            get {
                return this.codeField;
            }
            set {
                this.codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("projection", IsNullable=false)]
        public ProjectionType[] projections {
            get {
                return this.projectionsField;
            }
            set {
                this.projectionsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("format", IsNullable=false)]
        public FormatType[] formats {
            get {
                return this.formatsField;
            }
            set {
                this.formatsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Projection", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class ProjectionType {
        
        private string codeField;
        
        private string nameField;
        
        private string codespaceField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string code {
            get {
                return this.codeField;
            }
            set {
                this.codeField = value;
            }
        }
        
        /// <remarks/>
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string codespace {
            get {
                return this.codespaceField;
            }
            set {
                this.codespaceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.kxml.no/rest/1.0")]
    [System.Xml.Serialization.XmlRootAttribute("Link", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
    public partial class LinkType {
        
        private string hrefField;
        
        private string relField;
        
        private bool templatedField;
        
        private bool templatedFieldSpecified;
        
        private string typeField;
        
        private string deprecationField;
        
        private string nameField;
        
        private string titleField;
        
        /// <remarks/>
        public string href {
            get {
                return this.hrefField;
            }
            set {
                this.hrefField = value;
            }
        }
        
        /// <remarks/>
        public string rel {
            get {
                return this.relField;
            }
            set {
                this.relField = value;
            }
        }
        
        /// <remarks/>
        public bool templated {
            get {
                return this.templatedField;
            }
            set {
                this.templatedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool templatedSpecified {
            get {
                return this.templatedFieldSpecified;
            }
            set {
                this.templatedFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        public string deprecation {
            get {
                return this.deprecationField;
            }
            set {
                this.deprecationField = value;
            }
        }
        
        /// <remarks/>
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Format", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class FormatType {
        
        private string nameField;
        
        private string versionField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("CanDownloadRequest", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class CanDownloadRequestType {
        
        private string metadataUuidField;
        
        private string polygonField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string metadataUuid {
            get {
                return this.metadataUuidField;
            }
            set {
                this.metadataUuidField = value;
            }
        }
        
        /// <remarks/>
        public string polygon {
            get {
                return this.polygonField;
            }
            set {
                this.polygonField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("CanDownloadResponse", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class CanDownloadResponseType {
        
        private bool canDownloadField;
        
        private string messageField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public bool canDownload {
            get {
                return this.canDownloadField;
            }
            set {
                this.canDownloadField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Capabilities", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class CapabilitiesType {
        
        private bool supportsProjectionSelectionField;
        
        private bool supportsFormatSelectionField;
        
        private bool supportsPolygonSelectionField;
        
        private bool supportsAreaSelectionField;
        
        private string mapSelectionLayerField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public bool supportsProjectionSelection {
            get {
                return this.supportsProjectionSelectionField;
            }
            set {
                this.supportsProjectionSelectionField = value;
            }
        }
        
        /// <remarks/>
        public bool supportsFormatSelection {
            get {
                return this.supportsFormatSelectionField;
            }
            set {
                this.supportsFormatSelectionField = value;
            }
        }
        
        /// <remarks/>
        public bool supportsPolygonSelection {
            get {
                return this.supportsPolygonSelectionField;
            }
            set {
                this.supportsPolygonSelectionField = value;
            }
        }
        
        /// <remarks/>
        public bool supportsAreaSelection {
            get {
                return this.supportsAreaSelectionField;
            }
            set {
                this.supportsAreaSelectionField = value;
            }
        }
        
        /// <remarks/>
        public string mapSelectionLayer {
            get {
                return this.mapSelectionLayerField;
            }
            set {
                this.mapSelectionLayerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("File", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class FileType {
        
        private string downloadUrlField;
        
        private string fileSizeField;
        
        private string nameField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string downloadUrl {
            get {
                return this.downloadUrlField;
            }
            set {
                this.downloadUrlField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="integer")]
        public string fileSize {
            get {
                return this.fileSizeField;
            }
            set {
                this.fileSizeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Order", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderType {
        
        private string emailField;
        
        private OrderLineType orderLinesField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string email {
            get {
                return this.emailField;
            }
            set {
                this.emailField = value;
            }
        }
        
        /// <remarks/>
        public OrderLineType orderLines {
            get {
                return this.orderLinesField;
            }
            set {
                this.orderLinesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("OrderLine", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderLineType {
        
        private OrderAreaType[] areasField;
        
        private FormatType[] formatsField;
        
        private string metadataUuidField;
        
        private ProjectionType[] projectionsField;
        
        private string coordinatesField;
        
        private string coordinatesystemField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("orderarea", IsNullable=false)]
        public OrderAreaType[] areas {
            get {
                return this.areasField;
            }
            set {
                this.areasField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("format", IsNullable=false)]
        public FormatType[] formats {
            get {
                return this.formatsField;
            }
            set {
                this.formatsField = value;
            }
        }
        
        /// <remarks/>
        public string metadataUuid {
            get {
                return this.metadataUuidField;
            }
            set {
                this.metadataUuidField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("projection", IsNullable=false)]
        public ProjectionType[] projections {
            get {
                return this.projectionsField;
            }
            set {
                this.projectionsField = value;
            }
        }
        
        /// <remarks/>
        public string coordinates {
            get {
                return this.coordinatesField;
            }
            set {
                this.coordinatesField = value;
            }
        }
        
        /// <remarks/>
        public string coordinatesystem {
            get {
                return this.coordinatesystemField;
            }
            set {
                this.coordinatesystemField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("OrderArea", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderAreaType {
        
        private string codeField;
        
        private string nameField;
        
        private string typeField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string code {
            get {
                return this.codeField;
            }
            set {
                this.codeField = value;
            }
        }
        
        /// <remarks/>
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("OrderReceipt", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderReceiptType {
        
        private string referenceNumberField;
        
        private FileType[] filesField;
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        public string referenceNumber {
            get {
                return this.referenceNumberField;
            }
            set {
                this.referenceNumberField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("file", IsNullable=false)]
        public FileType[] files {
            get {
                return this.filesField;
            }
            set {
                this.filesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.kxml.no/rest/1.0")]
    [System.Xml.Serialization.XmlRootAttribute("LinkListe", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
    public partial class LinkListeType {
        
        private LinkType[] _linksField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("_links")]
        public LinkType[] _links {
            get {
                return this._linksField;
            }
            set {
                this._linksField = value;
            }
        }
    }
}
