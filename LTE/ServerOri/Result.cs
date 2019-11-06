/**
 * Autogenerated by Thrift Compiler (0.12.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace LTE
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class Result : TBase
  {
    private string _code;
    private string _token;
    private string _shpName;

    public bool Ok { get; set; }

    public string Msg { get; set; }

    public string Code
    {
      get
      {
        return _code;
      }
      set
      {
        __isset.code = true;
        this._code = value;
      }
    }

    public string Token
    {
      get
      {
        return _token;
      }
      set
      {
        __isset.token = true;
        this._token = value;
      }
    }

    public string ShpName
    {
      get
      {
        return _shpName;
      }
      set
      {
        __isset.shpName = true;
        this._shpName = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool code;
      public bool token;
      public bool shpName;
    }

    public Result() {
    }

    public Result(bool ok, string msg) : this() {
      this.Ok = ok;
      this.Msg = msg;
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_ok = false;
        bool isset_msg = false;
        TField field;
        iprot.ReadStructBegin();
        while (true)
        {
          field = iprot.ReadFieldBegin();
          if (field.Type == TType.Stop) { 
            break;
          }
          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.Bool) {
                Ok = iprot.ReadBool();
                isset_ok = true;
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                Msg = iprot.ReadString();
                isset_msg = true;
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.String) {
                Code = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.String) {
                Token = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.String) {
                ShpName = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            default: 
              TProtocolUtil.Skip(iprot, field.Type);
              break;
          }
          iprot.ReadFieldEnd();
        }
        iprot.ReadStructEnd();
        if (!isset_ok)
          throw new TProtocolException(TProtocolException.INVALID_DATA, "required field Ok not set");
        if (!isset_msg)
          throw new TProtocolException(TProtocolException.INVALID_DATA, "required field Msg not set");
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public void Write(TProtocol oprot) {
      oprot.IncrementRecursionDepth();
      try
      {
        TStruct struc = new TStruct("Result");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        field.Name = "ok";
        field.Type = TType.Bool;
        field.ID = 1;
        oprot.WriteFieldBegin(field);
        oprot.WriteBool(Ok);
        oprot.WriteFieldEnd();
        if (Msg == null)
          throw new TProtocolException(TProtocolException.INVALID_DATA, "required field Msg not set");
        field.Name = "msg";
        field.Type = TType.String;
        field.ID = 2;
        oprot.WriteFieldBegin(field);
        oprot.WriteString(Msg);
        oprot.WriteFieldEnd();
        if (Code != null && __isset.code) {
          field.Name = "code";
          field.Type = TType.String;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Code);
          oprot.WriteFieldEnd();
        }
        if (Token != null && __isset.token) {
          field.Name = "token";
          field.Type = TType.String;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Token);
          oprot.WriteFieldEnd();
        }
        if (ShpName != null && __isset.shpName) {
          field.Name = "shpName";
          field.Type = TType.String;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(ShpName);
          oprot.WriteFieldEnd();
        }
        oprot.WriteFieldStop();
        oprot.WriteStructEnd();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("Result(");
      __sb.Append(", Ok: ");
      __sb.Append(Ok);
      __sb.Append(", Msg: ");
      __sb.Append(Msg);
      if (Code != null && __isset.code) {
        __sb.Append(", Code: ");
        __sb.Append(Code);
      }
      if (Token != null && __isset.token) {
        __sb.Append(", Token: ");
        __sb.Append(Token);
      }
      if (ShpName != null && __isset.shpName) {
        __sb.Append(", ShpName: ");
        __sb.Append(ShpName);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}