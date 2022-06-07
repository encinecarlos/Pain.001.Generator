using Bogus;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Pain._001.Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        [HttpGet("{payments}")]
        public ActionResult<string> GeneratePaymentFile([FromRoute] int payments)
        {
            var faker = new Faker("en");
            var paymentList = new List<PaymentInstructionInformation3CH>();

            var painDocument = new Document
            {
                CstmrCdtTrfInitn = new CustomerCreditTransferInitiationV03CH
                {
                    GrpHdr = new GroupHeader32CH
                    {
                        MsgId = $"MSG-{Guid.NewGuid().ToString().Substring(0,5)}",
                        CreDtTm = DateTime.Now,
                        CtrlSum = payments * 500,
                        CtrlSumSpecified = true,
                        NbOfTxs = payments.ToString(),
                        InitgPty = new PartyIdentification32CH_NameAndId
                        {
                            Nm = faker.Name.FirstName()
                        }
                    },
                }
            };

            for (int i = 0; i < payments; i++)
            {
                var paymentInfo = new PaymentInstructionInformation3CH
                {
                    PmtInfId = $"PMT-{Guid.NewGuid().ToString().Substring(0,5)}",
                    PmtMtd = PaymentMethod3Code.TRF,
                    BtchBookgSpecified = true,
                    BtchBookg = true,
                    ReqdExctnDt = DateTime.Now,
                    Dbtr = new PartyIdentification32CH
                    {
                        Nm = faker.Name.FullName(),
                        PstlAdr = new PostalAddress6CH
                        {
                            Ctry = "CH",
                            AdrLine = new string[] {
                                faker.Address.StreetName()
                            }
                        }
                    },
                    DbtrAcct = new CashAccount16CH_IdTpCcy
                    {
                        Id = new AccountIdentification4ChoiceCH
                        {
                            Item = "CH1608750063000104015",
                        }
                    },
                    DbtrAgt = new BranchAndFinancialInstitutionIdentification4CH_BicOrClrId
                    {
                        FinInstnId = new FinancialInstitutionIdentification7CH_BicOrClrId
                        { 
                            BIC = faker.Finance.Bic()
                        }
                    },
                    CdtTrfTxInf = new CreditTransferTransactionInformation10CH[]
                    {
                        new CreditTransferTransactionInformation10CH
                        {
                            PmtId = new PaymentIdentification1
                            {
                                InstrId = "INSTRID-02-01",
                                EndToEndId = "ENDTOENDID-QRR"
                            },
                            Amt = new AmountType3Choice
                            {
                                Item = new ActiveOrHistoricCurrencyAndAmount
                                {
                                    Ccy = "CHF",
                                    Value = 500
                                }
                            },
                            Cdtr = new PartyIdentification32CH_Name
                            {
                                Nm = faker.Name.FullName(),
                                PstlAdr = new PostalAddress6CH
                                {
                                    StrtNm = faker.Address.StreetName(),
                                    BldgNb = faker.Random.Int(10, 50).ToString(),
                                    PstCd = faker.Address.CountryCode(),
                                    TwnNm = faker.Address.CitySuffix(),
                                    Ctry = "CH"
                                }
                            },
                            CdtrAcct = new CashAccount16CH_Id
                            {
                                Id = new AccountIdentification4ChoiceCH
                                {
                                    Item = "CH4608750063680674002"
                                }
                            },
                            RmtInf = new RemittanceInformation5CH
                            {
                                Strd = new StructuredRemittanceInformation7
                                {
                                    CdtrRefInf = new CreditorReferenceInformation2
                                    {
                                        Tp = new CreditorReferenceType2
                                        {
                                            CdOrPrtry = new CreditorReferenceType1Choice
                                            {
                                                Item = "QRR"
                                            }
                                        },
                                        Ref = "210000000003139471430009017"
                                    },
                                    AddtlRmtInf = new string[]
                                    {
                                        "ref info test"
                                    }
                                }
                            }
                        }
                    }
                };

                paymentList.Add(paymentInfo);
            }

            painDocument.CstmrCdtTrfInitn.PmtInf = paymentList.ToArray();

            var pain001 = new XmlSerializer(painDocument.GetType());

            using var ms = new MemoryStream();

            var xmlSettings = new XmlWriterSettings()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
            };

            using var writer = XmlWriter.Create(ms, xmlSettings);
            
            pain001.Serialize(writer, painDocument);

            byte[] buffer = ms.ToArray();

            var content = Encoding.UTF8.GetString(buffer);

            var file = Convert.ToBase64String(buffer);

            return Ok(file);

        }
    }
}
