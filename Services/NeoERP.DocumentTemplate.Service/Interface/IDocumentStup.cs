using NeoErp.Core.Models.CustomModels;
using NeoErp.Core.Domain;
using NeoERP.DocumentTemplate.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NeoERP.DocumentTemplate.Service.Interface
{
    public interface IDocumentStup
    {
        #region account
        string DeleteAccountSetupByAccCode(string accCode);

        AccountSetupModel GetAccountDataByAccCode(string acccode);

        string createNewAccountSetup(AccountSetupModel model);
        string udpateAccountSetup(AccountSetupModel model);

        List<AccountSetupModel> GetAccountListByGroupCode(string groupId);
        List<BudgetCenterSetupModel> GetBudgetCenterListByGroupCode(string groupId);
        List<BudgetCenterSetupModel> GetAllBudgetCenterList(string searchText);
        string DeleteBudgetCenterByBudgetCode(string budgetCode);
        BudgetCenterSetupModel GetBudgetCenterDetailByBudgetCode(string budgetcode);

        string createNewBudgetSetup(BudgetCenterSetupModel model);
        string udpateBudgetSetup(BudgetCenterSetupModel model);
        List<AccountSetupModel> GetAccountList(string searchtext);
        string GetNewAccountCode();
        #endregion

        #region Customer

        List<CustomerSetupModel> GetAccountListByCustomerCode(string groupId, string wholeSearchText);
        CustomerModels GetChildCustomerByCustomerCode(string customerCode);
        string createNewCustomerSetup(CustomerModels model);
        string createNewChildCustomerSetup(CustomerModels model);
        string updateCustomerSetup(CustomerModels model);
        string DeleteCustomerByCustomerCode(string custCode);
        string DeleteCustomerTreeByCustCode(string custCode);
        int? GetMaxCustomer();
        int? GetMaxChildCustomer();
        List<CountryModels> getAllCountry(string filter);
        List<BudgetCenter> getAllBudgetCenter(string filter);
        List<ItemSetupModel> getAllItemsForCustomerStock(string filter);

        List<LocationModels> getAllLocation(string filter);
        List<ZoneModels> getAllZones(string filter);
        List<RegionalModels> getAllRegions(string filter);
        List<DistrictModels> getAllDistricts(string filter);
        List<CityModels> getAllCities(string filter);
        List<PartyTypeModels> getAllPartyTypes(string filter);
        List<AgentModels> getAllAgents(string filter);
        List<Currency> getAllCurrency();
        List<BranchModels> getAllBranchs(string filter);
        //pricelist
        List<MasterFieldForUpdate> getAllPricelist(string filter);
        List<AccTypeModels> getAllAccountMaps(string filter);
        List<DivisionModels> getAllDivisions(string filter);
        List<DivisionModels> getAllComboDivisions(string filter);
        List<EmployeeCodeModels> getAllComboEmployees(string filter);
        List<DealerModels> getAllComboDealers(string filter);
        List<DivisionSetupModel> getAllDivisionschild(string groupId);
        List<DivisionSetupModel> getAllDivisionsList(string searchText);
        #endregion
        //Create Customer for Symphony and Opera
        string createNewCustomerSetup1(CustomerModels model);
        string createNewItemSetup1(ItemSetupModel model);
        List<CustomerSetupModel> GetAllAccountListByCustomerCode123(string searchText);

        #region Division Setup
        //Division createNewDivisionSetup
        DivisionSetupModel GetDivisionCenterDetailByDivisionCode(string divisionCode);
        string DeleteDivisionCenterByDivisionCode(string divisionCode);
        string createNewDivisionSetup(DivisionSetupModel model);
        string udpateDivisionSetup(DivisionSetupModel model);
        #endregion

        #region Branch Setup
        //Branch   
        string DeleteBranchCenterByBranchCode(string branchCode);
        string createNewBranchSetup(BranchSetupModel model);
        string udpateBranchSetup(BranchSetupModel model);
        BranchSetupModel GetBranchCenterDetailBybranchCode(string branchCode);
        List<BranchSetupModel> GetBranchCenterListByGroupCode(string groupId);
        List<BranchSetupModel> GetAllBranchCenterList(string searchText);
        #endregion

        #region Dealer Setup 
        string CreateNewCustomer1234(MappedCustomerModel model);
        List<CustomerSetupModel> GetAccountListByCustomerCode123();
        string createNewDealerSetup(DealerModel model);
        string udpateDealerSetup(DealerModel model);
        string DeleteDealerCenterByDealerCode(string dealerCode);
        List<DealerModel> GetDealerListByGroupCode(string groupId);
        DealerModel GetDealerDetailBydealerCode(string dealerCode);
        List<CustSubList> GetDealerMapped(string dealerCode);

        #endregion    
        string deletePartySetup(string partyCode);
        string updatePartyType(PartyTypeModel model);
        string createNewPaetyType(PartyTypeModel model);
        List<AccountCodeModels> getAllAccountCodeParty();
        List<PartyTypeModel> partyTypeList();
        #region Company Setup
        //Company setup   
        string createNewCompanySetup(CompanySetupModel model);
        string updateCompanySetup(CompanySetupModel model);
        CompanySetupModel GetCompnyDetailByCompanyCode(string cmpanyId);
        List<CompanySetupModel> getAllCompanychild();
        string DeleteCompanyByCompanyCode(string companyCode);
        #endregion

        #region Preference
        //Preference Setup  
        string createNewPreferenceSetup(PreferenceModel model);
        string updatePreferenceSetup(PreferenceModel model, User userInfo);
        List<PreferenceModel> getAllPreference();
        string DeletepreffBybranchCode(string companyCode);
        PreferenceModel GetPreferenceDetailByCompanyCode(string cmpanyId);
        List<CompanySetupModel> getCompanyPreff();

        List<BranchSetupModel> getBranchPreff(string COMPANY_CODE);
        List<CurrencymultiModel> getAllCurrencymulti();
        List<PreferenceModel> getFormLoad(User userInfo);
        #endregion

        #region Location
        string DeleteLocationSetupByLocationCode(string locationCode);

        LocationSetupModel GetLocationDataByLocationCode(string locationcode);

        string createNewLocationSetup(LocationSetupModel model);
        string udpateLocationSetup(LocationSetupModel model);

        List<LocationSetupModel> GetLocationListByGroupCode(string groupId);
        List<LocationSetupModel> GetAllLocationList(string searchText);
        #endregion

        #region Regional
        string DeleteRegionalSetupByRegionalCode(string regionCode);

        RegionalSetupModel GetRegionalDataByRegionalCode(string regioncode);

        string createNewRegionalSetup(RegionalSetupModel model);
        string udpateRegionalSetup(RegionalSetupModel model);

        List<RegionalSetupModel> GetRegionalListByGroupCode(string groupId);
        List<RegionalSetupModel> GetAllRegionalList(string searchText);
        #endregion

        #region Process GetProcessInputGriddata
        List<ProcessSetupModel> GetProcessListByprocessCode(string groupId);
        string DeleteProcessSetupByProcessCode(string processCode);

        ProcessSetupModel GetProcessDataByProcessCode(string processcode);

        string createNewProcessSetup(ProcessSetupModel model);
        string udpateProcessSetup(ProcessSetupModel model);

        List<ProcessSetupModel> GetProcessListByGroupCode(string groupId);
        List<RoutineInputModel> GetProcessInputGriddata();
        List<ProcessSetupModel> GetAllProcessList(string searchText);
        #endregion

        #region Resource
        string DeleteResourceSetupByResourceCode(string resourceCode);

        ResourceSetupModel GetResourceDataByResourceCode(string resourcecode);

        string createNewResourceSetup(ResourceSetupModel model);
        string udpateResourceSetup(ResourceSetupModel model);

        List<ResourceSetupModel> GetResourceListByGroupCode(string groupId);
        List<ResourceSetupModel> GetAllResourceList(string searchText);
        #endregion

        #region item
        ItemSetupModel GetItemDataByItemCode(string acccode);
        string GetItemDescByItemCode(string masterid);
        string GetMaxItemCode(string gFlag);
        String DeleteItemSetupByItemCode(string accCode);
        string createNewItemSetup(ItemSetupModel model);
        string udpateItemSetup(ItemSetupModel model);
        List<ItemSetupModel> GetItemListByGroupCode(string groupId);
        List<ItemSetupModel> GetAllItemList(string searchText);


        #endregion

        #region Supplier
        SuplierSetupModel GetSupplierDataBysupplierCode(string ItemCode);
        List<SuplierSetupModel> GetSupplyListByGroupCode(string groupId);
        string createNewSupplierSetup(SuplierSetupModalSet model);
        string udpateSupplierSetup(SuplierSetupModalSet model);
        string DeleteSupplierSetupBySupplierCode(string suppliercode);
        string getNewSupplierCode();
        SuplierSetupModel GetSupplierDataBySupplierCode(string SupplierCode);
        //string GetSupplierCodeByPreSupplierCode(string precode);
        List<SuplierSetupModel> GetAllSupplyList(string searchText);
        #endregion

        #region Area
        List<AreaModels> getAllAreaCodeDetail();
        string createNewAreaSetup(AreaModels model);
        string deleteAreaSetup(string areaCode);
        string updateAreaSetup(AreaModels model);
        string getMaxAreaCode();

        #endregion

        #region Agent
        List<AgentModels> getAllAgentCodeDetail();
        string createNewAgentSetup(AgentModels model);
        string deleteAgentSetup(string agentCode);
        string updateAgentSetup(AgentModels model);
        string getMaxAgentCode();

        #endregion

        #region Transporter
        List<TransporterModels> getAllTransporterCodeDetail();
        string createNewTransporterSetup(TransporterModels model);
        string deleteTransporterSetup(string areaCode);
        string updateTransporterSetup(TransporterModels model);
        string getMaxTransporterCode();


        #endregion
        string InsertQuickSetup(QuickSetupModel model);
        SubmitResponse SaveWebPrefrence(WebPrefrence model);

        #region Vehicel Setup 
        //Vehicle Setup  
        string deleteVehicleSetups(string vehicleCode);
        string createVehicleSetup(VehicleSetupModel model);
        string updateVehicleSetup(VehicleSetupModel model);
        List<VehicleSetupModel> getAllVehicle();
        string GetVehicleCode1(string vehicletype);
        #endregion
        #region TDS Setup  
        string getMaxTdsCode();
        List<TDSTypeModel> getAllTDS();
        string createTDSSetup(TDSTypeModel model);
        string updatetdsSetup(TDSTypeModel model);
        string deleteTDsSetup(string tdsCode);
        #endregion

        #region Priority Setup  
        List<PrioritySeupModel> getAllPriority();
        string createPriority(PrioritySeupModel model);
        string updatePrioritySetup(PrioritySeupModel model);
        string deletePrioritySetups(string priorityCode);
        #endregion

        #region Rejectalbe Item Setup 
        List<RejectableItem> getRejectlbleitems();
        string createRejectableItemSetup(RejectableItem model);
        string updateRejetableItemSetup(RejectableItem model);
        string deleteRejectableSetup(string itemId);
        #endregion

        #region Vehicel Registration setup
        //Vehicle Registration  GetAllVechileDtls12
        List<VehicleRegistrationModel> GetVehicleReg(string from);
        string getMaxTransactionNo(string gFlag);
        string createNewVehicleReg(VehicleRegistrationModel model);
        string updateVehicleReg(VehicleRegistrationModel model);
        string DeleteVehicleRegistration(string vehicleCode);
        VehicleRegistrationModel GetVehicleDetailBytrCode(string transactionCode);

        #endregion
        string CreateKYCForm(KYCFORM model);
        KYCFORM GetKYCFORM(string customerCode);


        #region ISSUE TYPE SETUP
        List<IssueType> GetSavedIssueType();

        string SaveIssueType(IssueTypeSetupModel typeModal);

        string UpdateIssueTypeSetup(IssueTypeSetupModel model);

        string DeleteIssueTypeSetups(string issueTypeCode);
        #endregion

        #region MEASUREMENT UNIT SETUP
        List<MeasurementUnit> GetAllMeasurementUnit();

        string SaveMeasurementUnit(MeasurementUnit unitModal);

        string UpdateMeasurementUnit(MeasurementUnit model);

        string DeleteMeasurementUnit(string unitCode);
        #endregion


        #region CITY SETUP

        List<DistrictModels> GetDistricts();

        List<CityModels> GetCities();

        string SaveCitySetup(CityModelForSave cityModal);

        string UpdateCitySetup(CityModelForSave citymodel);

        string DeleteCitySetup(string cityCode);
        #endregion

        #region Currency Setup
        //Currency Setup
        string deleteCurrencySetup(string currencyCode);
        string createCurrencySetup(CurrencySetupModel model);
        string updateCurrencySetup(CurrencySetupModel model);
        List<CurrencySetupModel> getAllCurrencyCode();
        #endregion

        #region Category Setup
        //Category Setup
        string deleteCategorySetup(string categoryCode);
        string createCategorySetup(CategorySetupModel model);
        string updateCategorySetup(CategorySetupModel model);
        List<CategorySetupModel> getAllCategoryCode();
        #endregion

        #region CHARGE TYPE SETUP
        List<ChargeSetupModel> GetCharges();

        string SaveChargeType(ChargeSetupModel chargeModal);

        string UpdateChargeSetup(ChargeSetupModel model);

        string DeleteChargeSetup(string chargeCode);

        #endregion

        #region scheme setup
        List<SchemeModels> getAllSchemeCodeDetail();
        string createNewSchemeSetup(SchemeModels model);
        string deleteSchemeSetup(string schemeCode);
        string updateSchemeSetup(SchemeModels model);
        string getMaxSchemeCode();

        List<SchemeModels> getAllScheme(string filter);
        string ImpactSchemeOnVoucherCustomer(SchemeImplementModel models, string formcode, string acccode, string chargecode, string chargeAmount);

        string ImpactSchemeOnVoucherPartyType(SchemeImplementModel models, string formcode, string acccode, string chargecode, string chargeAmount);
        #endregion


        List<CustomerSetupModel> GetAccountListByCustomerCodeByDealerCode(string CustomerCode);

        string ImplementScheme(string schemeCode);

        string ImpactSchemeOnVoucher(List<SchemeImplementModel> model, string formcode, string acccode, string chargecode, string chargeAmount);

        string GetcustomerNameByCode(string code);

        string GetParytTypeNameByCode(string code);
        #region InterestCalculation
        List<InterestCalculationResultModel> CalculateInterestByPara(InterestCalculationModel model);
        string CreateInterestImpact(List<InterestCalculationResultModel> result, InterestCalculcImpacttModel model);

        List<InterestCalcLogModel> GetInterestCalcLog();

        List<InterestCalcResDetailModel> CalculateInterestDetailsByPara(InterestCalculationModel model);
        #endregion



    }
}
