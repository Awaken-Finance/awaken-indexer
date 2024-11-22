namespace SwapIndexer;

public class AwakenSwapConst
{
    public const string AELF = "AELF";
    public const string tDVV = "tDVV";
    public const string tDVW = "tDVW";
    
    //AELF
    public const string MultiTokenContractAddress = "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE";
    
    //tDVV
    public const string MultiTokenContractAddressTDVV = "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX";
    public const string HooksContractAddressTDVV = "T3mdFC35CQSatUXQ5bQ886pULo2TnzS9rfXxmsoZSGnTq2a2S";
    public const string LimitOrderContractAddressTDVV = "BEakVbMWHXqQAn3oj3nj2dPk8jfFeJeTg9C99rPZiYTBhGB1a";
    //0.001
    public const double SwapContractFeeRateTDVVLevel1 = 0.001;
    public const string SwapContractAddressTDVVLevel1 = "hyiwdsbDnyoG1uZiw2JabQ4tLiWT6yAuDfNBFbHhCZwAqU1os";
    //0.003
    public const double SwapContractFeeRateTDVVLevel2 = 0.003;
    public const string SwapContractAddressTDVVLevel2 = "JvDB3rguLJtpFsovre8udJeXJLhsV1EPScGz2u1FFneahjBQm";
    //0.0005
    public const double SwapContractFeeRateTDVVLevel3 = 0.0005;
    public const string SwapContractAddressTDVVLevel3 = "83ju3fGGnvQzCmtjApUTwvBpuLQLQvt5biNMv4FXCvWKdZgJf";
    //0.03
    public const double SwapContractFeeRateTDVVLevel4 = 0.03;
    public const string SwapContractAddressTDVVLevel4 = "2q7NLAr6eqF4CTsnNeXnBZ9k4XcmiUeM61CLWYaym6WsUmbg1k";
    //0.05
    public const double SwapContractFeeRateTDVVLevel5 = 0.05;
    public const string SwapContractAddressTDVVLevel5 = "UYdd84gLMsVdHrgkr3ogqe1ukhKwen8oj32Ks4J1dg6KH9PYC";
        
    //tDVW
    public const string MultiTokenContractAddressTDVW = "ASh2Wt7nSEmYqnGxPPzp4pnVDU4uhj1XW9Se5VeZcX2UDdyjx";
    public const string HooksContractAddressTDVW = "2vahJs5WeWVJruzd1DuTAu3TwK8jktpJ2NNeALJJWEbPQCUW4Y";
    public const string LimitOrderContractAddressTDVW = "2BC4BosozC1x27izqrSFJ51gYYtyVByjKGZvmitY7EBFDDPYHN";
    //0.003
    public const double SwapContractFeeRateTDVWLevel1 = 0.003;
    public const string SwapContractAddressTDVWLevel1 = "2YnkipJ9mty5r6tpTWQAwnomeeKUT7qCWLHKaSeV1fejYEyCdX";
    //0.0005
    public const double SwapContractFeeRateTDVWLevel2 = 0.0005;
    public const string SwapContractAddressTDVWLevel2 = "fGa81UPViGsVvTM13zuAAwk1QHovL3oSqTrCznitS4hAawPpk";
    //0.001
    public const double SwapContractFeeRateTDVWLevel3 = 0.001;
    public const string SwapContractAddressTDVWLevel3 = "LzkrbEK2zweeuE4P8Y23BMiFY2oiKMWyHuy5hBBbF1pAPD2hh";
    //0.03
    public const double SwapContractFeeRateTDVWLevel4 = 0.03;
    public const string SwapContractAddressTDVWLevel4 = "EG73zzQqC8JencoFEgCtrEUvMBS2zT22xoRse72XkyhuuhyTC";
    //0.05
    public const double SwapContractFeeRateTDVWLevel5 = 0.05;
    public const string SwapContractAddressTDVWLevel5 = "23dh2s1mXnswi4yNW7eWNKWy7iac8KrXJYitECgUctgfwjeZwP";
   
    public static readonly List<string> SortedTokenWeights = new List<string>
    {
        "USDT",
        "USDC",
        "DAI",
        "ELF",
        "ETH",
        "BNB"
    };
    
    public const string Asc = "asc";
    public const string Ascend = "ascend";

    public const string BaseToken = "USDT";
    public const string QuoteToken = "ELF";
    public const int BaseTokenDecimal = 6;
    public const int QuoteTokenDecimal = 8;
    
}
