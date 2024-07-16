using AElf;
using AElf.Types;

namespace Swap.Indexer;

public static class AddressHelper
{
    public static Address ConvertVirtualAddressToContractAddress(
        Hash virtualAddress,
        Address contractAddress)
    {
        return Address.FromPublicKey(contractAddress.Value.Concat<byte>((IEnumerable<byte>) virtualAddress.Value.ToByteArray().ComputeHash()).ToArray<byte>());
    }
}