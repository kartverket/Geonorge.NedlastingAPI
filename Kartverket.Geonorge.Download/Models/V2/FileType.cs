using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models.V2
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/2.0")]
    [System.Xml.Serialization.XmlRootAttribute("File", Namespace = "http://skjema.geonorge.no/SOSI/download/2.0", IsNullable = false)]
    public partial class FileType
    {

        private string downloadUrlField;

        private string fileSizeField;

        private string nameField;

        /// <remarks/>
        /// <summary>
        /// Url to download file
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string downloadUrl
        {
            get
            {
                return this.downloadUrlField;
            }
            set
            {
                this.downloadUrlField = value;
            }
        }

        /// <remarks/>
        /// <summary>
        /// Not available
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer", IsNullable = true)]
        public string fileSize
        {
            get
            {
                return this.fileSizeField;
            }
            set
            {
                this.fileSizeField = value;
            }
        }

        /// <remarks/>
        /// <summary>
        /// The filename
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

}