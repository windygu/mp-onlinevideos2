﻿namespace ZDFMediathek2009.Code.DTO
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlRoot(Namespace="", IsNullable=false), XmlType(AnonymousType=true), GeneratedCode("xsd", "2.0.50727.3038"), DebuggerStepThrough, DesignerCategory("code")]
    public class feedlist
    {
        private ZDFMediathek2009.Code.DTO.value[] valueField;

        [XmlElement("value")]
        public ZDFMediathek2009.Code.DTO.value[] value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}

