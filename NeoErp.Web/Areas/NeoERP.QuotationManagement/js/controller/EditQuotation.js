QMModule.controller('EditQuotation', function ($scope, $rootScope, $http, $filter, $timeout, $location, $httpParamSerializer) {
    $scope.pageName = "Add Quotation";
    $scope.saveAction = "Save";
    var url = new URL(window.location.href);
    var id = url.searchParams.get("id");

    $scope.idShowHide = false;

    $("#englishdatedocument").kendoDatePicker();
    $("#validDt").kendoDatePicker();
    $("#deliveryDt").kendoDatePicker();
    $scope.productFormList = [];
    $scope.counterProduct = 1;
    $scope.showCustomerDetails = false;
    $scope.showSpecificationDetail = false;
    $scope.selectedCurrency = "";


    $scope.setInitialWidth = function () {
        $(".table-container").css("width", "98%");
    };
    $scope.toggleDetails = function () {
        $scope.showSpecificationDetail = !$scope.showSpecificationDetail;
    };
    $scope.TENDER_NO = "";
    $http.get('/api/QuotationApi/getTenderNo',)
        .then(function (response) {
            $scope.TENDER_NO = response.data[0].TENDER_NO;
        })
        .catch(function (error) {
            console.error('Error fetching ID:', error);
        });
    $scope.selectedItem = null;

    $scope.ItemSelect = {
        dataTextField: "ItemDescription",
        dataValueField: "ItemCode",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Group...</strong></div>',
        placeholder: "Select Item...",
        autoClose: true,
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/QuotationApi/ItemDetails",
                    dataType: "json"
                }
            }
        }
    };
    $scope.currencySelect = {
        dataTextField: "name",
        dataValueField: "name",
        dataSource: {
            transport: {
                read: {
                    url: "https://gist.githubusercontent.com/aaronhayes/5fef481815ac75f771d37b16d16d35c9/raw/edbec8eea5cc9ace57a79409cc390b7b9bcf24f6/currencies.json",
                    dataType: "json"
                }
            }
        },
        optionLabel: "Currency" // Optional: Add a default option label
    };

    $scope.addProduct = function () {
        $http.get("/api/QuotationApi/ItemDetails")
            .then(function (response) {
                // Assuming response.data is an array of objects with 'ItemDescription' and 'ItemCode' properties
                $scope.ItemSelect.dataSource.data = response.data;
                $scope.productFormList.push({
                    ID: $scope.counterProduct,
                    ItemDescription: "",
                    UNIT: "",
                    IMAGE: "",
                    QUANTITY: "",
                    UNIT_PRICE: "",
                    AMOUNT: "",
                    REMARKS: ""
                });
            })
            .catch(function (error) {
                //console.error('Error fetching item details:', error);
                displayPopupNotification("Error fetching item details", "error");
            });
    };
    function formatDate(dateString) {
        var date = new Date(dateString);
        return $filter('date')(date, 'dd-MMM-yyyy');
    }
    $scope.updateUnit = function (product) {
        if (product && product.ItemDescription) {
            // Find the item with the matching ItemCode
            var selectedItem = $scope.ItemSelect.dataSource.data.find(function (item) {
                return item.ItemCode === product.ItemDescription;
            });

            // If a matching item is found, set the UNIT to the ItemUnit of the selected item
            if (selectedItem) {
                product.UNIT = selectedItem.ItemUnit;
                product.SPECIFICATION = selectedItem.SPECIFICATION;

            }
        } else {
            product.UNIT = ""; // Set UNIT to empty string if product or ItemCode is not provided
        }
    };

    $scope.updateAmount = function (product) {
        if (product.UNIT_PRICE && product.QUANTITY) {
            product.AMOUNT = product.UNIT_PRICE * product.QUANTITY;
        } else {
            product.AMOUNT = null;
        }
    };
    $scope.addRow = function () {
        var maxId = Math.max(...$scope.productFormList.map(product => product.ID));
        $scope.counterProduct = maxId !== -Infinity ? maxId + 1 : 1;
        $scope.productFormList.push({
            ID: $scope.counterProduct,
            ItemDescription: "",
            UNIT: "",
            QUANTITY: "",
            UNIT_PRICE: "",
            AMOUNT: "",
            IMAGE: "",
            REMARKS: ""
        });
    };
    $scope.deleteRow = function (index) {
        var itemId = $scope.productFormList[index].TID;
        var tenderNo = $scope.TENDER_NO;
        var action = $scope.saveAction;

        if (itemId) {
            if (action == 'Update') {
                $http.post('/api/QuotationApi/updateItemsById?tenderNo=' + tenderNo + '&id=' + itemId)
                    .then(function (response) {
                        var message = response.data.MESSAGE;
                        var deletedProduct = $scope.productFormList.splice(index, 1)[0];
                        delete deletedProduct.ID;
                        //$scope.productFormList.splice(index, 1);
                        displayPopupNotification(message, "success");
                    }).catch(function (error) {
                        var message = 'Error in displaying project!!';
                        displayPopupNotification(message, "error");
                    });
            }
        } else {
            var deletedProduct = $scope.productFormList.splice(index, 1)[0];
            delete deletedProduct.ID;
            $scope.productFormList.splice(index, 1); // Remove the row at the specified index
            if (index === 0) {
                $scope.addProduct(); // Reload the Add Product page if the first row is deleted
            }
        }
    };
    $scope.updateQuantity = function () {
        var totalQty = 0;
        angular.forEach($scope.productFormList, function (item) {
            totalQty += item.QUANTITY ?? 0;
        });
        $scope.totalQty = totalQty ?? totalQty.toFixed(2);
    };
    $scope.updateQuantity();

    

    $scope.saveData = function () {
        var formData = {
            ID: $scope.ID,
            TENDER_NO: $scope.TENDER_NO,
            ISSUE_DATE: $('#englishdatedocument').val(),
            VALID_DATE: $('#validDt').val(),
            REMARKS: $('#remarks').val(),
            Items: []
        };

        var count = 0;
        var ProductEmpty = false;
        // Loop over the productFormList if it's not empty
        if ($scope.productFormList && $scope.productFormList.length > 0) {
            var totalFiles = $scope.productFormList.length;

            angular.forEach($scope.productFormList, function (itemList) {
                var fileInput = document.getElementById('image_' + itemList.ID);
                var file = fileInput.files[0];
                if (itemList.ItemDescription == "" || typeof itemList.ItemDescription === "undefined" || itemList.ItemDescription === null) {
                    displayPopupNotification("Product Name is required", "warning");
                    ProductEmpty = true;
                    return;// Set flag to true if any rate is empty
                }
                if (!ProductEmpty) {
                    if (file) {
                        var reader = new FileReader();
                        reader.onload = function () {
                            var itemListData = {
                                ID: itemList.TID,
                                ITEM_CODE: itemList.ItemDescription,
                                SPECIFICATION: itemList.SPECIFICATION,
                                IMAGE: reader.result.split(',')[1],
                                UNIT: itemList.UNIT,
                                IMAGE_NAME: itemList.IMAGE_NAME,
                                QUANTITY: itemList.QUANTITY,
                                CATEGORY: itemList.CATEGORY,
                                BRAND_NAME: itemList.BRAND_NAME,
                                INTERFACE: itemList.INTERFACE,
                                TYPE: itemList.TYPE,
                                LAMINATION: itemList.LAMINATION,
                                ITEM_SIZE: itemList.ITEM_SIZE,
                                THICKNESS: itemList.THICKNESS,
                                COLOR: itemList.COLOR,
                                GRADE: itemList.GRADE,
                                SIZE_LENGTH: itemList.SIZE_LENGTH,
                                SIZE_WIDTH: itemList.SIZE_WIDTH,
                            };
                            formData.Items.push(itemListData);
                            count++;

                            if (count === totalFiles) {
                                saveFormData(formData);
                            }
                        };
                        reader.onerror = function (error) {
                            displayPopupNotification("Error reading file!!", "error");
                        };

                        reader.readAsDataURL(file); // Convert file to base64
                    } else {
                        var itemListData = {
                            ID: itemList.TID,
                            ITEM_CODE: itemList.ItemDescription,
                            SPECIFICATION: itemList.SPECIFICATION,
                            IMAGE: null,
                            UNIT: itemList.UNIT,
                            IMAGE_NAME: itemList.IMAGE_NAME,
                            QUANTITY: itemList.QUANTITY,
                            CATEGORY: itemList.CATEGORY,
                            BRAND_NAME: itemList.BRAND_NAME,
                            INTERFACE: itemList.INTERFACE,
                            TYPE: itemList.TYPE,
                            LAMINATION: itemList.LAMINATION,
                            ITEM_SIZE: itemList.ITEM_SIZE,
                            THICKNESS: itemList.THICKNESS,
                            COLOR: itemList.COLOR,
                            GRADE: itemList.GRADE,
                            SIZE_LENGTH: itemList.SIZE_LENGTH,
                            SIZE_WIDTH: itemList.SIZE_WIDTH,
                        };
                        formData.Items.push(itemListData);
                        count++;

                        if (count === totalFiles) {
                            saveFormData(formData);
                        }
                    }
                }
            });
        }
    };

    function saveFormData(formData) {
        $http.post('/api/QuotationApi/SaveItemData', formData)
            .then(function (response) {
                var message = response.data.message;
                $scope.createPanel = false;
                $scope.tablePanel = true;
                displayPopupNotification(message, "success");
                setTimeout(function () {
                    window.location.href = "/QuotationManagement/Home/Index#!QM/QuotationSetup"
                }, 2000);
            })
            .catch(function (error) {
                var message = error;
                displayPopupNotification(message, "error");
            });
    }

    $scope.Cancel = function () {
        $("#englishdatedocument").data("kendoDatePicker").value(null);
        $("#validDt").data("kendoDatePicker").value(null);
        $("#nepaliDate").val('');
        // Clear the content of productFormList
        $scope.productFormList.forEach(function (product) {
            product.ItemDescription = '';
            product.SPECIFICATION = '';
            product.CATEGORY = '';
            product.BRAND_NAME = '';
            product.INTERFACE = '';
            product.TYPE = '';
            product.LAMINATION = '';
            product.ITEM_SIZE = '';
            product.THICKNESS = '';
            product.COLOR = '';
            product.GRADE = '';
            product.SIZE_LENGHT = '';
            product.SIZE_WIDTH = '';
            product.IMAGE = ''; // Clear the image field
            product.UNIT = '';
            product.QUANTITY = '';
        });
        $scope.showSpecificationDetail = false;
        // Clear the file input value (if supported, this might not work in all browsers)
        var fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(function (input) {
            input.value = '';
        });
        window.location.href = "/QuotationManagement/Home/Index#!QM/QuotationSetup"


    };


    $scope.generatedUrl = '';

    $scope.generateLink = function () {
        $http.get('/api/QuotationApi/getTenderId?tenderNo=' + $scope.TENDER_NO)
            .then(function (response) {
                $scope.ID = response.data[0].ID;
                var linkeUrl = window.location.protocol + "//" + window.location.host + "/Quotation/Index?qo=" + $scope.ID;
                $scope.generatedUrl = linkeUrl;
            })
            .catch(function (error) {
                displayPopupNotification("Error fetching ID", "error");
            });
    };

    // Handle click event for the edit button

        $http.get('/api/QuotationApi/GetQuotationById?tenderNo=' + id)
            .then(function (response) {
                var quotation = response.data[0];
                $scope.ID = quotation.ID;
                $scope.TENDER_NO = quotation.TENDER_NO;
                var issueDate = new Date(quotation.ISSUE_DATE);
                var validDate = new Date(quotation.VALID_DATE);
                var issueDate = $filter('date')(new Date(quotation.ISSUE_DATE), 'dd-MMM-yyyy');
                var validDate = $filter('date')(new Date(quotation.VALID_DATE), 'dd-MMM-yyyy');

                // Set values for input fields with specific IDs
                $('#englishdatedocument').val(issueDate);
                $('#nepaliDate').val(quotation.NEPALI_DATE);
                $("#validDt").val(validDate);

                $scope.TXT_REMARKS = quotation.REMARKS;
                var id = 1;
                $scope.panelMode = 'edit';
                $scope.saveAction = "Update";
                $scope.createEdit = true; // Corrected typo here
                $scope.productFormList = [];
                if (quotation.Items.length === 0) {
                    // If there are no items, call addProduct directly
                    $scope.addProduct();
                } else {
                    for (var i = 0; i < quotation.Items.length; i++) {
                        var itemList = quotation.Items[i];
                        var imageUrl = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.QuotationManagement/Image/Items/" + itemList.IMAGE;
                        $scope.productFormList.push({
                            TID: itemList.ID,
                            ID: id,
                            ItemDescription: itemList.ITEM_CODE,
                            SPECIFICATION: itemList.SPECIFICATION,
                            IMAGE: itemList.IMAGE,
                            IMAGE_NAME: itemList.IMAGE,
                            IMAGE_LINK: imageUrl,
                            UNIT: itemList.UNIT,
                            QUANTITY: itemList.QUANTITY,
                            CATEGORY: itemList.CATEGORY,
                            BRAND_NAME: itemList.BRAND_NAME,
                            INTERFACE: itemList.INTERFACE,
                            TYPE: itemList.TYPE,
                            LAMINATION: itemList.LAMINATION,
                            ITEM_SIZE: itemList.ITEM_SIZE,
                            THICKNESS: itemList.THICKNESS,
                            COLOR: itemList.COLOR,
                            GRADE: itemList.GRADE,
                            SIZE_LENGTH: itemList.SIZE_LENGTH,
                            SIZE_WIDTH: itemList.SIZE_WIDTH,
                        });
                        id++;
                    }
                }
                $scope.createPanel = true;
                $scope.tablePanel = false;

                // After populating data, trigger select events
                setTimeout(function () {
                    for (let i = 0; i < quotation.Items.length; i++) {
                        var currentItem = quotation.Items[i];
                        var currentItemCode = currentItem.ITEM_CODE;
                        // Check if the element exists before attempting to trigger the select event
                        var dropdownElement = $("#item_" + id).data("kendoDropDownList");
                        if (dropdownElement) {
                            dropdownElement.value(currentItemCode);
                        }
                        id++;
                    }
                }, 200);
            })
            .catch(function (error) {
                var message = 'Error in displaying quotation!!';
                displayPopupNotification(message, "error");
            });

    $scope.openImage = function (imageUrl) {
        window.open(imageUrl, '_blank');
    };
    $scope.getItemByCode = function (itemCode, product) {
        var filteredItems = $filter('filter')($scope.ItemSelect.dataSource.data, { ItemCode: itemCode });
        if (filteredItems.length > 0) {
            var selectedItem = filteredItems[0]; // Get the first matching item
            product.ItemDescription = selectedItem.ItemDescription;
            product.Unit = selectedItem.ItemUnit;
        } else {
            // If no item found, you may want to clear the properties
            product.ItemDescription = null;
            product.Unit = null;
        }
    };

});

