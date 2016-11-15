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
namespace Geonorge.NedlastingApi.V2 {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Area", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class AreaType {
        
        /// <remarks/>
        public string type;
        
        /// <remarks/>
        public string name;
        
        /// <remarks/>
        public string code;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("projection", IsNullable=false)]
        public ProjectionType[] projections;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("format", IsNullable=false)]
        public FormatType[] formats;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Projection", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class ProjectionType {
        
        /// <remarks/>
        public string code;
        
        /// <remarks/>
        public string name;
        
        /// <remarks/>
        public string codespace;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.kxml.no/rest/1.0")]
    [System.Xml.Serialization.XmlRootAttribute("Link", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
    public partial class LinkType {
        
        /// <remarks/>
        public string href;
        
        /// <remarks/>
        public string rel;
        
        /// <remarks/>
        public bool templated;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool templatedSpecified;
        
        /// <remarks/>
        public string type;
        
        /// <remarks/>
        public string deprecation;
        
        /// <remarks/>
        public string name;
        
        /// <remarks/>
        public string title;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Format", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class FormatType {
        
        /// <remarks/>
        public string name;
        
        /// <remarks/>
        public string version;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("CanDownloadRequest", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class CanDownloadRequestType {
        
        /// <remarks/>
        public string metadataUuid;
        
        /// <remarks/>
        public string polygon;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("CanDownloadResponse", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class CanDownloadResponseType {
        
        /// <remarks/>
        public bool canDownload;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string message;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Capabilities", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class CapabilitiesType {
        
        /// <remarks/>
        public bool supportsProjectionSelection;
        
        /// <remarks/>
        public bool supportsFormatSelection;
        
        /// <remarks/>
        public bool supportsPolygonSelection;
        
        /// <remarks/>
        public bool supportsAreaSelection;
        
        /// <remarks/>
        public string mapSelectionLayer;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("File", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class FileType {
        
        /// <remarks/>
        public string downloadUrl;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="integer")]
        public string fileSize;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string name;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string fileId;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string metadataUuid;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string area;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string coordinates;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string projection;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string format;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string status;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("Order", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderType {
        
        /// <remarks/>
        public string email;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("orderline", IsNullable=false)]
        public OrderLineType[] orderLines;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("OrderLine", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderLineType {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("orderarea", IsNullable=false)]
        public OrderAreaType[] areas;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("format", IsNullable=false)]
        public FormatType[] formats;
        
        /// <remarks/>
        public string metadataUuid;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("projection", IsNullable=false)]
        public ProjectionType[] projections;
        
        /// <remarks/>
        public string coordinates;
        
        /// <remarks/>
        public string coordinatesystem;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("OrderArea", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderAreaType {
        
        /// <remarks/>
        public string code;
        
        /// <remarks/>
        public string name;
        
        /// <remarks/>
        public string type;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("OrderReceipt", Namespace="http://skjema.geonorge.no/SOSI/tjenestespesifikasjon/nedlastingapi/2.0", IsNullable=false)]
    public partial class OrderReceiptType {
        
        /// <remarks/>
        public string referenceNumber;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("file", IsNullable=false)]
        public FileType[] files;
        
        /// <remarks/>
        public string email;
        
        /// <remarks/>
        public System.DateTime orderDate;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
        public LinkType[] _links;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.kxml.no/rest/1.0")]
    [System.Xml.Serialization.XmlRootAttribute("LinkListe", Namespace="http://www.kxml.no/rest/1.0", IsNullable=false)]
    public partial class LinkListeType {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("_links")]
        public LinkType[] _links;
    }
}