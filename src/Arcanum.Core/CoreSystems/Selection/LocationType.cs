namespace Arcanum.Core.CoreSystems.Selection;

[Flags]
public enum LocationType
{
   Sea = 1,
   Lake = 2,
   Wasteland = 4,
   NotOwnable = 8,
   Land = 16,
   All = Sea | Lake | Wasteland | NotOwnable | Land,
}