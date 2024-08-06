// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: awaken_order_contract.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace Awaken.Contracts.Order {

  #region Events
  public partial class LimitOrderCreated : aelf::IEvent<LimitOrderCreated>
  {
    public global::System.Collections.Generic.IEnumerable<LimitOrderCreated> GetIndexed()
    {
      return new List<LimitOrderCreated>
      {
      };
    }

    public LimitOrderCreated GetNonIndexed()
    {
      return new LimitOrderCreated
      {
        AmountIn = AmountIn,
        SymbolIn = SymbolIn,
        AmountOut = AmountOut,
        SymbolOut = SymbolOut,
        Deadline = Deadline,
        CommitTime = CommitTime,
        OrderId = OrderId,
        Maker = Maker,
      };
    }
  }

  public partial class LimitOrderFilled : aelf::IEvent<LimitOrderFilled>
  {
    public global::System.Collections.Generic.IEnumerable<LimitOrderFilled> GetIndexed()
    {
      return new List<LimitOrderFilled>
      {
      };
    }

    public LimitOrderFilled GetNonIndexed()
    {
      return new LimitOrderFilled
      {
        OrderId = OrderId,
        FillTime = FillTime,
        AmountInFilled = AmountInFilled,
        AmountOutFilled = AmountOutFilled,
        Taker = Taker,
      };
    }
  }

  public partial class LimitOrderCancelled : aelf::IEvent<LimitOrderCancelled>
  {
    public global::System.Collections.Generic.IEnumerable<LimitOrderCancelled> GetIndexed()
    {
      return new List<LimitOrderCancelled>
      {
      };
    }

    public LimitOrderCancelled GetNonIndexed()
    {
      return new LimitOrderCancelled
      {
        OrderId = OrderId,
        CancelTime = CancelTime,
      };
    }
  }

  public partial class LimitOrderRemoved : aelf::IEvent<LimitOrderRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<LimitOrderRemoved> GetIndexed()
    {
      return new List<LimitOrderRemoved>
      {
      };
    }

    public LimitOrderRemoved GetNonIndexed()
    {
      return new LimitOrderRemoved
      {
        OrderId = OrderId,
        RemoveTime = RemoveTime,
      };
    }
  }

  #endregion
  public static partial class AwakenOrderContractContainer
  {
    static readonly string __ServiceName = "AwakenOrderContract";

    #region Marshallers
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.InitializeInput> __Marshaller_InitializeInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.InitializeInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Address> __Marshaller_aelf_Address = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Address.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.SetOrderBookConfigInput> __Marshaller_SetOrderBookConfigInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.SetOrderBookConfigInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.CommitLimitOrderInput> __Marshaller_CommitLimitOrderInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.CommitLimitOrderInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.CancelLimitOrderInput> __Marshaller_CancelLimitOrderInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.CancelLimitOrderInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.FillLimitOrderInput> __Marshaller_FillLimitOrderInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.FillLimitOrderInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.GetFillResultInput> __Marshaller_GetFillResultInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.GetFillResultInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.GetFillResultOutput> __Marshaller_GetFillResultOutput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.GetFillResultOutput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.GetBestSellPriceInput> __Marshaller_GetBestSellPriceInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.GetBestSellPriceInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.GetBestSellPriceOutput> __Marshaller_GetBestSellPriceOutput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.GetBestSellPriceOutput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.GetOrderBookConfigOutput> __Marshaller_GetOrderBookConfigOutput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.GetOrderBookConfigOutput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Int64Value> __Marshaller_google_protobuf_Int64Value = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Int64Value.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.PriceBook> __Marshaller_PriceBook = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.PriceBook.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.OrderBook> __Marshaller_OrderBook = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.OrderBook.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.UserLimitOrder> __Marshaller_UserLimitOrder = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.UserLimitOrder.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Awaken.Contracts.Order.CalculatePriceInput> __Marshaller_CalculatePriceInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Awaken.Contracts.Order.CalculatePriceInput.Parser.ParseFrom);
    #endregion

    #region Methods
    static readonly aelf::Method<global::Awaken.Contracts.Order.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Initialize = new aelf::Method<global::Awaken.Contracts.Order.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Initialize",
        __Marshaller_InitializeInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SetAdmin = new aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SetAdmin",
        __Marshaller_aelf_Address,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SetHooksContract = new aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SetHooksContract",
        __Marshaller_aelf_Address,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Order.SetOrderBookConfigInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SetOrderBookConfig = new aelf::Method<global::Awaken.Contracts.Order.SetOrderBookConfigInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SetOrderBookConfig",
        __Marshaller_SetOrderBookConfigInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Order.CommitLimitOrderInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_CommitLimitOrder = new aelf::Method<global::Awaken.Contracts.Order.CommitLimitOrderInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "CommitLimitOrder",
        __Marshaller_CommitLimitOrderInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Order.CancelLimitOrderInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_CancelLimitOrder = new aelf::Method<global::Awaken.Contracts.Order.CancelLimitOrderInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "CancelLimitOrder",
        __Marshaller_CancelLimitOrderInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Order.FillLimitOrderInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_FillLimitOrder = new aelf::Method<global::Awaken.Contracts.Order.FillLimitOrderInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "FillLimitOrder",
        __Marshaller_FillLimitOrderInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Awaken.Contracts.Order.GetFillResultInput, global::Awaken.Contracts.Order.GetFillResultOutput> __Method_GetFillResult = new aelf::Method<global::Awaken.Contracts.Order.GetFillResultInput, global::Awaken.Contracts.Order.GetFillResultOutput>(
        aelf::MethodType.View,
        __ServiceName,
        "GetFillResult",
        __Marshaller_GetFillResultInput,
        __Marshaller_GetFillResultOutput);

    static readonly aelf::Method<global::Awaken.Contracts.Order.GetBestSellPriceInput, global::Awaken.Contracts.Order.GetBestSellPriceOutput> __Method_GetBestSellPrice = new aelf::Method<global::Awaken.Contracts.Order.GetBestSellPriceInput, global::Awaken.Contracts.Order.GetBestSellPriceOutput>(
        aelf::MethodType.View,
        __ServiceName,
        "GetBestSellPrice",
        __Marshaller_GetBestSellPriceInput,
        __Marshaller_GetBestSellPriceOutput);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address> __Method_GetAdmin = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address>(
        aelf::MethodType.View,
        __ServiceName,
        "GetAdmin",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_aelf_Address);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address> __Method_GetHooksContract = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address>(
        aelf::MethodType.View,
        __ServiceName,
        "GetHooksContract",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_aelf_Address);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Awaken.Contracts.Order.GetOrderBookConfigOutput> __Method_GetOrderBookConfig = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Awaken.Contracts.Order.GetOrderBookConfigOutput>(
        aelf::MethodType.View,
        __ServiceName,
        "GetOrderBookConfig",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_GetOrderBookConfigOutput);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Awaken.Contracts.Order.PriceBook> __Method_GetPriceBook = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Awaken.Contracts.Order.PriceBook>(
        aelf::MethodType.View,
        __ServiceName,
        "GetPriceBook",
        __Marshaller_google_protobuf_Int64Value,
        __Marshaller_PriceBook);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Awaken.Contracts.Order.OrderBook> __Method_GetOrderBook = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Awaken.Contracts.Order.OrderBook>(
        aelf::MethodType.View,
        __ServiceName,
        "GetOrderBook",
        __Marshaller_google_protobuf_Int64Value,
        __Marshaller_OrderBook);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Awaken.Contracts.Order.UserLimitOrder> __Method_GetUserLimitOrder = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Awaken.Contracts.Order.UserLimitOrder>(
        aelf::MethodType.View,
        __ServiceName,
        "GetUserLimitOrder",
        __Marshaller_google_protobuf_Int64Value,
        __Marshaller_UserLimitOrder);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Google.Protobuf.WellKnownTypes.Int64Value> __Method_GetOrderBookIdByOrderId = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::Google.Protobuf.WellKnownTypes.Int64Value>(
        aelf::MethodType.View,
        __ServiceName,
        "GetOrderBookIdByOrderId",
        __Marshaller_google_protobuf_Int64Value,
        __Marshaller_google_protobuf_Int64Value);

    static readonly aelf::Method<global::Awaken.Contracts.Order.CalculatePriceInput, global::Google.Protobuf.WellKnownTypes.Int64Value> __Method_CalculatePrice = new aelf::Method<global::Awaken.Contracts.Order.CalculatePriceInput, global::Google.Protobuf.WellKnownTypes.Int64Value>(
        aelf::MethodType.View,
        __ServiceName,
        "CalculatePrice",
        __Marshaller_CalculatePriceInput,
        __Marshaller_google_protobuf_Int64Value);

    #endregion

    #region Descriptors
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Awaken.Contracts.Order.AwakenOrderContractReflection.Descriptor.Services[0]; }
    }

    public static global::System.Collections.Generic.IReadOnlyList<global::Google.Protobuf.Reflection.ServiceDescriptor> Descriptors
    {
      get
      {
        return new global::System.Collections.Generic.List<global::Google.Protobuf.Reflection.ServiceDescriptor>()
        {
          global::AElf.Standards.ACS12.Acs12Reflection.Descriptor.Services[0],
          global::Awaken.Contracts.Order.AwakenOrderContractReflection.Descriptor.Services[0],
        };
      }
    }
    #endregion

    /// <summary>Base class for the contract of AwakenOrderContract</summary>
    // public abstract partial class AwakenOrderContractBase : AElf.Sdk.CSharp.CSharpSmartContract<Awaken.Contracts.Order.AwakenOrderContractState>
    // {
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Initialize(global::Awaken.Contracts.Order.InitializeInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty SetAdmin(global::AElf.Types.Address input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty SetHooksContract(global::AElf.Types.Address input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty SetOrderBookConfig(global::Awaken.Contracts.Order.SetOrderBookConfigInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty CommitLimitOrder(global::Awaken.Contracts.Order.CommitLimitOrderInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty CancelLimitOrder(global::Awaken.Contracts.Order.CancelLimitOrderInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty FillLimitOrder(global::Awaken.Contracts.Order.FillLimitOrderInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Awaken.Contracts.Order.GetFillResultOutput GetFillResult(global::Awaken.Contracts.Order.GetFillResultInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Awaken.Contracts.Order.GetBestSellPriceOutput GetBestSellPrice(global::Awaken.Contracts.Order.GetBestSellPriceInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::AElf.Types.Address GetAdmin(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::AElf.Types.Address GetHooksContract(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Awaken.Contracts.Order.GetOrderBookConfigOutput GetOrderBookConfig(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Awaken.Contracts.Order.PriceBook GetPriceBook(global::Google.Protobuf.WellKnownTypes.Int64Value input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Awaken.Contracts.Order.OrderBook GetOrderBook(global::Google.Protobuf.WellKnownTypes.Int64Value input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Awaken.Contracts.Order.UserLimitOrder GetUserLimitOrder(global::Google.Protobuf.WellKnownTypes.Int64Value input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Int64Value GetOrderBookIdByOrderId(global::Google.Protobuf.WellKnownTypes.Int64Value input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Int64Value CalculatePrice(global::Awaken.Contracts.Order.CalculatePriceInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    // }

    // public static aelf::ServerServiceDefinition BindService(AwakenOrderContractBase serviceImpl)
    // {
    //   return aelf::ServerServiceDefinition.CreateBuilder()
    //       .AddDescriptors(Descriptors)
    //       .AddMethod(__Method_Initialize, serviceImpl.Initialize)
    //       .AddMethod(__Method_SetAdmin, serviceImpl.SetAdmin)
    //       .AddMethod(__Method_SetHooksContract, serviceImpl.SetHooksContract)
    //       .AddMethod(__Method_SetOrderBookConfig, serviceImpl.SetOrderBookConfig)
    //       .AddMethod(__Method_CommitLimitOrder, serviceImpl.CommitLimitOrder)
    //       .AddMethod(__Method_CancelLimitOrder, serviceImpl.CancelLimitOrder)
    //       .AddMethod(__Method_FillLimitOrder, serviceImpl.FillLimitOrder)
    //       .AddMethod(__Method_GetFillResult, serviceImpl.GetFillResult)
    //       .AddMethod(__Method_GetBestSellPrice, serviceImpl.GetBestSellPrice)
    //       .AddMethod(__Method_GetAdmin, serviceImpl.GetAdmin)
    //       .AddMethod(__Method_GetHooksContract, serviceImpl.GetHooksContract)
    //       .AddMethod(__Method_GetOrderBookConfig, serviceImpl.GetOrderBookConfig)
    //       .AddMethod(__Method_GetPriceBook, serviceImpl.GetPriceBook)
    //       .AddMethod(__Method_GetOrderBook, serviceImpl.GetOrderBook)
    //       .AddMethod(__Method_GetUserLimitOrder, serviceImpl.GetUserLimitOrder)
    //       .AddMethod(__Method_GetOrderBookIdByOrderId, serviceImpl.GetOrderBookIdByOrderId)
    //       .AddMethod(__Method_CalculatePrice, serviceImpl.CalculatePrice).Build();
    // }

  }
}
#endregion

