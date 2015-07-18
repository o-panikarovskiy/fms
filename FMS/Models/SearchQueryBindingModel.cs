using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Models
{
    public class SearchPersonBindingModel
    {
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public PersonType? Type { get; set; }
        public PersonCategory? Category { get; set; }
        public int? Citizenship { get; set; }
        public string Address { get; set; }
        public int? DocType { get; set; }
        public string DocNo { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class SearchDocumentBindingModel
    {
        public string DocNo { get; set; }
        public bool IsChecked { get; set; }
    }

    public class SearchAPDocBindingModel : SearchDocumentBindingModel
    {
        public DateTime? StDateCreate { get; set; }
        public DateTime? EndDateCreate { get; set; }
        public int? Article { get; set; }
        public int? CrimeType { get; set; }
        public int? StateDepartment { get; set; }
        public int? DocStatus { get; set; }
        public DateTime? StDecreeDate { get; set; }
        public DateTime? EndDecreeDate { get; set; }
        public int? DecreeStr { get; set; }
        public int? PenaltyType { get; set; }
    }

    public class SearchMUBindingModel : SearchDocumentBindingModel
    {
        public int? CardMark { get; set; }
        public int? PurposeOfEntry { get; set; }
        public int? PrimaryExtend { get; set; }
        public int? KPP { get; set; }
        public DateTime? StIncomeDate { get; set; }
        public DateTime? EndIncomeDate { get; set; }
        public DateTime? StIssueDate { get; set; }
        public DateTime? EndIssueDate { get; set; }
        public DateTime? RegDateFrom { get; set; }
        public DateTime? RegDateTo { get; set; }
    }

    public class SearchRVPBindingModel : SearchDocumentBindingModel
    {
        public string DecisionNo { get; set; }
        public string RvpNo { get; set; }
        public int? AdmissionReason { get; set; }
        public int? DecisionBase { get; set; }
        public int? DecisionUser { get; set; }
        public DateTime? StDateOfReceipt { get; set; }
        public DateTime? EndDateOfReceipt { get; set; }
        public DateTime? StDecisionDate { get; set; }
        public DateTime? EndDecisionDate { get; set; }
        public DateTime? StPrintDate { get; set; }
        public DateTime? EndPrintDate { get; set; }
        public DateTime? StActualDate { get; set; }
        public DateTime? EndActualDate { get; set; }

    }

    public class SearchVNGBindingModel : SearchDocumentBindingModel
    {
        public string DocNo2 { get; set; }
        public string VngNo { get; set; }
        public int? DocActionType { get; set; }
        public int? DocAdmission { get; set; }
        public int? DecisionType { get; set; }
        public int? Series { get; set; }
        public DateTime? StDateOfReceipt { get; set; }
        public DateTime? EndDateOfReceipt { get; set; }
        public DateTime? StDecisionDate { get; set; }
        public DateTime? EndDecisionDate { get; set; }
        public DateTime? StIssueDate { get; set; }
        public DateTime? EndIssueDate { get; set; }
        public DateTime? StActualDate { get; set; }
        public DateTime? EndActualDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

    }

    public class SearchCtzBindingModel : SearchDocumentBindingModel
    {
        public string DecisionNo { get; set; }
        public int? DocActionType { get; set; }
        public int? AdmissionReason { get; set; }
        public int? Decision { get; set; }
        public int? DecisionBase { get; set; }
        public DateTime? StAdmissionDate { get; set; }
        public DateTime? EndAdmissionDate { get; set; }
        public DateTime? StDecisionDate { get; set; }
        public DateTime? EndDecisionDate { get; set; }

    }

    public class SearchDocumnet
    {
        public SearchAPDocBindingModel Ap { get; set; }
        public SearchMUBindingModel Mu { get; set; }
        public SearchRVPBindingModel Rvp { get; set; }
        public SearchVNGBindingModel Vng { get; set; }
        public SearchCtzBindingModel Ctz { get; set; }
    }

    public class SearchQueryBindingModel
    {
        public SearchPersonBindingModel Person { get; set; }
        public SearchDocumnet Docs { get; set; }
    }
}
