QMModule.controller('tenderSetup', function ($scope, $rootScope, $http, $filter, $timeout) {

    $scope.AddTender = function () {
        $('#tenderModal').modal('show');
    }
    $scope.saveTender = function () {
        var formData = {
            ID:$scope.ID,
            PREFIX: $scope.PREFIX,
            SUFFIX: $scope.SUFFIX,
            BODY_LENGTH: $scope.BODY_LENGTH
        }
        $http.post('/api/QuotationApi/saveTender', formData)
            .then(function (response) {
                var message = response.data.message; // Extract message from response
                displayPopupNotification(message, "success");
                setTimeout(function () {
                    window.location.reload();
                },5000)
            })
            .catch(function (error) {
                displayPopupNotification(error,"error");
            })
    }

    $http.get('/api/QuotationApi/TenderDetails')
        .then(function (response) {
            var tenders = response.data;
            if (tenders && tenders.length > 0) {
                $scope.dataSource.data(tenders);
            }
        })
        .catch(function (error) {
            displayPopupNotification(error, "error");
        })
    $scope.dataSource = new kendo.data.DataSource({
        data: [], // Initially empty
    });
    $("#kGrid").kendoGrid({
        dataSource: $scope.dataSource,
        height: 400,
        sortable: true, // Enable sorting
        pageable: {
            refresh: true,
            pageSizes: true
        },
        toolbar: ["excel"/*, "pdf"*/],
        excel: {
            fileName: "Tender Details.xlsx",
            allPages: true
        },
        resizable: true, // Enable column resizing
        columns: [
            { field: "ID", title: "S.N", width: 90, type: "string" },
            { field: "PREFIX", title: "Prefix Text", width: 200, type: "string" },
            {
                field: "SUFFIX", title: "Suffix Text", width: 200, type: "string",
            },
            {
                field: "BODY_LENGTH", title: "Body Length", width: 150, type: "string",
            },
            {
                field: "CREATED_DATE", title: "Created Date", width: 150, type: "string",
                template: "#=kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') == null?'':kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') #",

            },
            {
                title: "Actions",
                width: 120,
                template: "<a class='btn btn-sm btn-info view-btn' data-id='#= ID #'><i class='fa fa-eye'></i></a>&nbsp;<a class='btn btn-sm btn-warning edit-btn' data-id='#= ID #'><i class='fa fa-edit'></i></a>&nbsp;<a class='btn btn-sm btn-danger delete-btn' data-id='#= ID #'><i class='fa fa-trash'></i></a>"
            }
        ]
    });
    // Handle click event for the delete button
    $("#kGrid").on("click", ".delete-btn", function () {
        var deleteButton = $(this);
        var id = $(this).data("id");

        // Create the popover element with custom HTML content
        var popoverContent = `
        <div class="popover-delete-confirm">
            <p>Delete?</p>
            <div class="popover-buttons">
                <button type="button" class="btn btn-danger confirm-delete">Yes</button>
                <button type="button" class="btn btn-secondary cancel-delete">No</button>
            </div>
        </div>
    `;
        deleteButton.popover({
            container: 'body',
            placement: 'bottom',
            html: true,
            content: popoverContent
        });

        // Show popover
        deleteButton.popover('show');

        // Handle click event on the "Yes" button
        $(document).on('click', '.confirm-delete', function () {
            $http.post('/api/QuotationApi/deleteTenderId?tenderNo=' + id)
                .then(function (response) {
                    var message = response.data.MESSAGE;
                    displayPopupNotification(message, "success");
                    setTimeout(function () {
                        window.location.reload();
                    }, 5000)
                }).catch(function (error) {
                    var message = 'Error in displaying tender no!!'; // Extract message from response
                    displayPopupNotification(message, "error");
                });
            deleteButton.popover('hide');
        });

        // Handle click event on the "No" button
        $(document).on('click', '.cancel-delete', function () {
            // Hide the popover
            deleteButton.popover('hide');
        });

    });
    //view button
    $("#kGrid").on("click", ".view-btn", function () {
        var viewBtn = $(this);
        var id = $(this).data("id");
        $http.get('/api/QuotationApi/getTenderById?tenderNo=' + id)
            .then(function (response) {
                $scope.panelMode = 'view';
                var tenders = response.data[0];
                $scope.PREFIX = tenders.PREFIX;
                $scope.SUFFIX = tenders.SUFFIX;
                $scope.BODY_LENGTH = tenders.BODY_LENGTH;
                $('#tenderModal').modal('show');
            }).catch(function (error) {
                var message = 'Error in displaying tender no!!'; 
                displayPopupNotification(message, "error");
            })
    })
    $("#kGrid").on("click", ".edit-btn", function () {
        var editBtn = $(this);
        var id = $(this).data("id");
        $http.get('/api/QuotationApi/getTenderById?tenderNo=' + id)
            .then(function (response) {
                var tenders = response.data[0];
                $scope.ID = tenders.ID;
                $scope.PREFIX = tenders.PREFIX;
                $scope.SUFFIX = tenders.SUFFIX;
                $scope.BODY_LENGTH = tenders.BODY_LENGTH;
                $('#tenderModal').modal('show');
            }).catch(function (error) {
                var message = 'Error in displaying tender no!!';
                displayPopupNotification(message, "error");
            })
    })
    //edit button 

    $scope.ItemCancel = function () {
        window.location.reload(); 
    }
});

