#region

using Arcanum.Core.GameObjects.BaseTypes;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Economy;

public interface IEnumDemand
{
   public void GetData(out Enum value, out float amount);
}

public interface IEnumDemand<TrueType, in DataType> : IEu5Object<TrueType>, IEnumDemand
   where DataType : struct, Enum where TrueType : IEu5Object<TrueType>, new()
{
   public void SetData(DataType value, float amount);
}

public interface IIEu5ObjectDemand
{
   public void GetData(out IEu5Object value, out float amount);
}

public interface IIEu5ObjectDemand<TrueType, in DataType> : IIEu5ObjectDemand, IEu5Object<TrueType>
   where DataType : IEu5Object<DataType>, new()
   where TrueType : IEu5Object<TrueType>, new()
{
   public void SetData(DataType value, float amount);
}