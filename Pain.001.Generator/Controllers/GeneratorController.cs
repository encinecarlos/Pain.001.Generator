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
        public ActionResult GeneratePaymentFile([FromRoute] int payments)
        {
            var faker = new Faker("en");
            var paymentList = new List<PaymentInstructionInformation3CH>();

            var painDocument = new Document
            {
                CstmrCdtTrfInitn = new CustomerCreditTransferInitiationV03CH
                {
                    GrpHdr = new GroupHeader32CH
                    {
                        MsgId = Guid.NewGuid().ToString(),
                        CreDtTm = DateTime.Now,
                        CtrlSum = payments * 10,
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
                    PmtInfId = Guid.NewGuid().ToString(),
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
                            Item = "CH5481230000001998736",
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
                                    Value = i * 10
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
                                    Ctry = faker.Address.City()
                                }
                            },
                            CdtrAcct = new CashAccount16CH_Id
                            {
                                Id = new AccountIdentification4ChoiceCH
                                {
                                    Item = faker.Finance.Iban(countryCode: "CH")
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
                Encoding = System.Text.Encoding.UTF8
            };

            var writer = XmlWriter.Create(ms, xmlSettings);

            pain001.Serialize(writer, painDocument);

            byte[] buffer = ms.ToArray();

            var content = Encoding.UTF8.GetString(buffer);

            var file = Convert.ToBase64String(buffer);

            return Ok(content);

        }
    }
}
