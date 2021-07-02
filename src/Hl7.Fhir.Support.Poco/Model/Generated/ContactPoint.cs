// <auto-generated/>
// Contents of: hl7.fhir.r5.core version: 4.6.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation;

/*
  Copyright (c) 2011+, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  
*/

namespace Hl7.Fhir.Model
{
  /// <summary>
  /// Details of a Technology mediated contact point (phone, fax, email, etc.)
  /// </summary>
  [Serializable]
  [DataContract]
  [FhirType("ContactPoint","http://hl7.org/fhir/StructureDefinition/ContactPoint")]
  public partial class ContactPoint : Hl7.Fhir.Model.DataType
  {
    /// <summary>
    /// FHIR Type Name
    /// </summary>
    public override string TypeName { get { return "ContactPoint"; } }

    /// <summary>
    /// Telecommunications form for contact point.
    /// (url: http://hl7.org/fhir/ValueSet/contact-point-system)
    /// (system: http://hl7.org/fhir/contact-point-system)
    /// </summary>
    [FhirEnumeration("ContactPointSystem")]
    public enum ContactPointSystem
    {
      /// <summary>
      /// The value is a telephone number used for voice calls. Use of full international numbers starting with + is recommended to enable automatic dialing support but not required.
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("phone", "http://hl7.org/fhir/contact-point-system"), Description("Phone")]
      Phone,
      /// <summary>
      /// The value is a fax machine. Use of full international numbers starting with + is recommended to enable automatic dialing support but not required.
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("fax", "http://hl7.org/fhir/contact-point-system"), Description("Fax")]
      Fax,
      /// <summary>
      /// The value is an email address.
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("email", "http://hl7.org/fhir/contact-point-system"), Description("Email")]
      Email,
      /// <summary>
      /// The value is a pager number. These may be local pager numbers that are only usable on a particular pager system.
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("pager", "http://hl7.org/fhir/contact-point-system"), Description("Pager")]
      Pager,
      /// <summary>
      /// A contact that is not a phone, fax, pager or email address and is expressed as a URL.  This is intended for various institutional or personal contacts including web sites, blogs, Skype, Twitter, Facebook, etc. Do not use for email addresses.
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("url", "http://hl7.org/fhir/contact-point-system"), Description("URL")]
      Url,
      /// <summary>
      /// A contact that can be used for sending an sms message (e.g. mobile phones, some landlines).
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("sms", "http://hl7.org/fhir/contact-point-system"), Description("SMS")]
      Sms,
      /// <summary>
      /// A contact that is not a phone, fax, page or email address and is not expressible as a URL.  E.g. Internal mail address.  This SHOULD NOT be used for contacts that are expressible as a URL (e.g. Skype, Twitter, Facebook, etc.)  Extensions may be used to distinguish "other" contact types.
      /// (system: http://hl7.org/fhir/contact-point-system)
      /// </summary>
      [EnumLiteral("other", "http://hl7.org/fhir/contact-point-system"), Description("Other")]
      Other,
    }

    /// <summary>
    /// Use of contact point.
    /// (url: http://hl7.org/fhir/ValueSet/contact-point-use)
    /// (system: http://hl7.org/fhir/contact-point-use)
    /// </summary>
    [FhirEnumeration("ContactPointUse")]
    public enum ContactPointUse
    {
      /// <summary>
      /// A communication contact point at a home; attempted contacts for business purposes might intrude privacy and chances are one will contact family or other household members instead of the person one wishes to call. Typically used with urgent cases, or if no other contacts are available.
      /// (system: http://hl7.org/fhir/contact-point-use)
      /// </summary>
      [EnumLiteral("home", "http://hl7.org/fhir/contact-point-use"), Description("Home")]
      Home,
      /// <summary>
      /// An office contact point. First choice for business related contacts during business hours.
      /// (system: http://hl7.org/fhir/contact-point-use)
      /// </summary>
      [EnumLiteral("work", "http://hl7.org/fhir/contact-point-use"), Description("Work")]
      Work,
      /// <summary>
      /// A temporary contact point. The period can provide more detailed information.
      /// (system: http://hl7.org/fhir/contact-point-use)
      /// </summary>
      [EnumLiteral("temp", "http://hl7.org/fhir/contact-point-use"), Description("Temp")]
      Temp,
      /// <summary>
      /// This contact point is no longer in use (or was never correct, but retained for records).
      /// (system: http://hl7.org/fhir/contact-point-use)
      /// </summary>
      [EnumLiteral("old", "http://hl7.org/fhir/contact-point-use"), Description("Old")]
      Old,
      /// <summary>
      /// A telecommunication device that moves and stays with its owner. May have characteristics of all other use codes, suitable for urgent matters, not the first choice for routine business.
      /// (system: http://hl7.org/fhir/contact-point-use)
      /// </summary>
      [EnumLiteral("mobile", "http://hl7.org/fhir/contact-point-use"), Description("Mobile")]
      Mobile,
    }

    /// <summary>
    /// phone | fax | email | pager | url | sms | other
    /// </summary>
    [FhirElement("system", InSummary=true, Order=30)]
    [DeclaredType(Type = typeof(Code))]
    [DataMember]
    public Code<Hl7.Fhir.Model.ContactPoint.ContactPointSystem> SystemElement
    {
      get { return _SystemElement; }
      set { _SystemElement = value; OnPropertyChanged("SystemElement"); }
    }

    private Code<Hl7.Fhir.Model.ContactPoint.ContactPointSystem> _SystemElement;

    /// <summary>
    /// phone | fax | email | pager | url | sms | other
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public Hl7.Fhir.Model.ContactPoint.ContactPointSystem? System
    {
      get { return SystemElement != null ? SystemElement.Value : null; }
      set
      {
        if (value == null)
          SystemElement = null;
        else
          SystemElement = new Code<Hl7.Fhir.Model.ContactPoint.ContactPointSystem>(value);
        OnPropertyChanged("System");
      }
    }

    /// <summary>
    /// The actual contact point details
    /// </summary>
    [FhirElement("value", InSummary=true, Order=40)]
    [DataMember]
    public Hl7.Fhir.Model.FhirString ValueElement
    {
      get { return _ValueElement; }
      set { _ValueElement = value; OnPropertyChanged("ValueElement"); }
    }

    private Hl7.Fhir.Model.FhirString _ValueElement;

    /// <summary>
    /// The actual contact point details
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public string Value
    {
      get { return ValueElement != null ? ValueElement.Value : null; }
      set
      {
        if (value == null)
          ValueElement = null;
        else
          ValueElement = new Hl7.Fhir.Model.FhirString(value);
        OnPropertyChanged("Value");
      }
    }

    /// <summary>
    /// home | work | temp | old | mobile - purpose of this contact point
    /// </summary>
    [FhirElement("use", InSummary=true, Order=50)]
    [DeclaredType(Type = typeof(Code))]
    [DataMember]
    public Code<Hl7.Fhir.Model.ContactPoint.ContactPointUse> UseElement
    {
      get { return _UseElement; }
      set { _UseElement = value; OnPropertyChanged("UseElement"); }
    }

    private Code<Hl7.Fhir.Model.ContactPoint.ContactPointUse> _UseElement;

    /// <summary>
    /// home | work | temp | old | mobile - purpose of this contact point
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public Hl7.Fhir.Model.ContactPoint.ContactPointUse? Use
    {
      get { return UseElement != null ? UseElement.Value : null; }
      set
      {
        if (value == null)
          UseElement = null;
        else
          UseElement = new Code<Hl7.Fhir.Model.ContactPoint.ContactPointUse>(value);
        OnPropertyChanged("Use");
      }
    }

    /// <summary>
    /// Specify preferred order of use (1 = highest)
    /// </summary>
    [FhirElement("rank", InSummary=true, Order=60)]
    [DataMember]
    public Hl7.Fhir.Model.PositiveInt RankElement
    {
      get { return _RankElement; }
      set { _RankElement = value; OnPropertyChanged("RankElement"); }
    }

    private Hl7.Fhir.Model.PositiveInt _RankElement;

    /// <summary>
    /// Specify preferred order of use (1 = highest)
    /// </summary>
    /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
    [IgnoreDataMember]
    public int? Rank
    {
      get { return RankElement != null ? RankElement.Value : null; }
      set
      {
        if (value == null)
          RankElement = null;
        else
          RankElement = new Hl7.Fhir.Model.PositiveInt(value);
        OnPropertyChanged("Rank");
      }
    }

    /// <summary>
    /// Time period when the contact point was/is in use
    /// </summary>
    [FhirElement("period", InSummary=true, Order=70)]
    [DataMember]
    public Hl7.Fhir.Model.Period Period
    {
      get { return _Period; }
      set { _Period = value; OnPropertyChanged("Period"); }
    }

    private Hl7.Fhir.Model.Period _Period;

    public override IDeepCopyable CopyTo(IDeepCopyable other)
    {
      var dest = other as ContactPoint;

      if (dest == null)
      {
        throw new ArgumentException("Can only copy to an object of the same type", "other");
      }

      base.CopyTo(dest);
      if(SystemElement != null) dest.SystemElement = (Code<Hl7.Fhir.Model.ContactPoint.ContactPointSystem>)SystemElement.DeepCopy();
      if(ValueElement != null) dest.ValueElement = (Hl7.Fhir.Model.FhirString)ValueElement.DeepCopy();
      if(UseElement != null) dest.UseElement = (Code<Hl7.Fhir.Model.ContactPoint.ContactPointUse>)UseElement.DeepCopy();
      if(RankElement != null) dest.RankElement = (Hl7.Fhir.Model.PositiveInt)RankElement.DeepCopy();
      if(Period != null) dest.Period = (Hl7.Fhir.Model.Period)Period.DeepCopy();
      return dest;
    }

    public override IDeepCopyable DeepCopy()
    {
      return CopyTo(new ContactPoint());
    }

    public override bool Matches(IDeepComparable other)
    {
      var otherT = other as ContactPoint;
      if(otherT == null) return false;

      if(!base.Matches(otherT)) return false;
      if( !DeepComparable.Matches(SystemElement, otherT.SystemElement)) return false;
      if( !DeepComparable.Matches(ValueElement, otherT.ValueElement)) return false;
      if( !DeepComparable.Matches(UseElement, otherT.UseElement)) return false;
      if( !DeepComparable.Matches(RankElement, otherT.RankElement)) return false;
      if( !DeepComparable.Matches(Period, otherT.Period)) return false;

      return true;
    }

    public override bool IsExactly(IDeepComparable other)
    {
      var otherT = other as ContactPoint;
      if(otherT == null) return false;

      if(!base.IsExactly(otherT)) return false;
      if( !DeepComparable.IsExactly(SystemElement, otherT.SystemElement)) return false;
      if( !DeepComparable.IsExactly(ValueElement, otherT.ValueElement)) return false;
      if( !DeepComparable.IsExactly(UseElement, otherT.UseElement)) return false;
      if( !DeepComparable.IsExactly(RankElement, otherT.RankElement)) return false;
      if( !DeepComparable.IsExactly(Period, otherT.Period)) return false;

      return true;
    }

    [IgnoreDataMember]
    public override IEnumerable<Base> Children
    {
      get
      {
        foreach (var item in base.Children) yield return item;
        if (SystemElement != null) yield return SystemElement;
        if (ValueElement != null) yield return ValueElement;
        if (UseElement != null) yield return UseElement;
        if (RankElement != null) yield return RankElement;
        if (Period != null) yield return Period;
      }
    }

    [IgnoreDataMember]
    public override IEnumerable<ElementValue> NamedChildren
    {
      get
      {
        foreach (var item in base.NamedChildren) yield return item;
        if (SystemElement != null) yield return new ElementValue("system", SystemElement);
        if (ValueElement != null) yield return new ElementValue("value", ValueElement);
        if (UseElement != null) yield return new ElementValue("use", UseElement);
        if (RankElement != null) yield return new ElementValue("rank", RankElement);
        if (Period != null) yield return new ElementValue("period", Period);
      }
    }

    public override bool TryGetValue(string key, out object value)
    {
      switch (key)
      {
        case "system":
          value = SystemElement;
          return SystemElement is not null;
        case "value":
          value = ValueElement;
          return ValueElement is not null;
        case "use":
          value = UseElement;
          return UseElement is not null;
        case "rank":
          value = RankElement;
          return RankElement is not null;
        case "period":
          value = Period;
          return Period is not null;
        default:
          return base.TryGetValue(key, out value);
      };

    }

    protected override IEnumerable<KeyValuePair<string, object>> GetElementPairs()
    {
      foreach (var kvp in base.GetElementPairs()) yield return kvp;
      if (SystemElement is not null) yield return new KeyValuePair<string,object>("system",SystemElement);
      if (ValueElement is not null) yield return new KeyValuePair<string,object>("value",ValueElement);
      if (UseElement is not null) yield return new KeyValuePair<string,object>("use",UseElement);
      if (RankElement is not null) yield return new KeyValuePair<string,object>("rank",RankElement);
      if (Period is not null) yield return new KeyValuePair<string,object>("period",Period);
    }

    public override void EnumerateElements(Action<string,object> callback)
    {
      base.EnumerateElements(callback);
      if (SystemElement is not null) callback("system",SystemElement);
      if (ValueElement is not null) callback("value",ValueElement);
      if (UseElement is not null) callback("use",UseElement);
      if (RankElement is not null) callback("rank",RankElement);
      if (Period is not null) callback("period",Period);
    }

  }

}

// end of file
