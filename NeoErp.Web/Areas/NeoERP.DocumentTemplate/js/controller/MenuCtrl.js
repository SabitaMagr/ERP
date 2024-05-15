
DTModule.controller('MenuCtrl', function ($scope, $http, $routeParams, $window, $filter, menuService) {

    
    $scope.MenuData = [];
    $scope.InventoryMenuData = [];
    $scope.SetupMenuData = [];
    $scope.InventoryDraftData = [];
    $scope.InventorySavedDraftData = [];
    $scope.FOLDER_NAME = "";
    $scope.ORDER_NO = "";
    $scope.ICON = "";
    $scope.new_folder = true;
    $scope.WebManagement = [];
    $scope.target = "";
    var d1 = $.Deferred();

    //$scope.FinanceVoucherDraftData = [];
    //$scope.FinanceVoucherSavedDraftData = [];
    //$scope.SalesDraftData = [];
    //$scope.SalesSavedDraftData = [];

    setTimeout(function () {
        var url = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getSalesVerificationUserWise";
        $http({
            method: 'GET',
            url: url,
            data: null
        }).then(function successCallback(response) {
            // old
            //$("#lstSalesCheck").hide();
            //$("#lstSalesAuthorize").hide();
            //$("#lstSalesPost").hide();

            //new
            $("a:contains('/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/04')").hide();
            $("a:contains('/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/05')").hide();
            $("a:contains('/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/06')").hide();
            for (var i = 0; i < response.data.length; i++) {
                if (response.data[i] == "Document Check") {
                    $("a:contains('/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/04')").show();
                    //$("#lstSalesCheck").show();
                }
                if (response.data[i] == "Document Verification") {
                    $("a:contains('/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/05')").show();
                    // $("#lstSalesAuthorize").show();
                }
                if (response.data[i] == "Document Post") {
                    $("a:contains('/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/06')").show();
                    // $("#lstSalesPost").show();
                }
            }
        });
    }, 100);

    // old

    //var getSalesVerificationUserWise = function () {        
    //}
    //getSalesVerificationUserWise();

    //Custom Menu Definition Start 

   //$scope.CustomMenu = {};
   // $scope.CustomMenu = {
   //     "formName": "Custom Form",
   //     "groupSkuFlag": "G",
   //     "masterFormCode": "00",
   //     "iconPath":"",
   //     "Items": [{
   //         "formName": "Post Data Check",
   //         "preFormCode": "00",
   //         "groupSkuFlag": "I"
   //     }]
   // };

    //Custom Menu Definition End

    $scope.getMenuItems = function () {
        menuService.getMenuDetail().then(function (d) {
            $scope.MenuData = d.data;
            $.each($scope.MenuData, function (id, val) {
                val["COLOR"] = getRandomColor();
                val["ICON_PATH"] = generatRandomIcon();
            });
        }, function () {
            alert("Error at: Get All Menu Items")
        });
        menuService.getDynamicMenus($scope.moduleCode).then(function (res) {
            $scope.menuList = res.data;
            $scope.menuList.push({
                "formName": "Custom Form",
                "groupSkuFlag": "G",
                "masterFormCode": "00",
                "iconPath": "fa fa-plus-square",
                "Items": [
                {
                    "formName": "Post & Open Dated Cheque",
                    "preFormCode": "00",
                    "groupSkuFlag": "I",
                    "iconPath":"fa fa-check-square",
                    "urlForSetup": "/DocumentTemplate/Home/Index#!DT/postDatedCheque"
                },
                {
                    "formName": "Column Settings",
                    "preFormCode": "00",
                    "groupSkuFlag": "I",
                    "iconPath": "fa fa-code",
                    "urlForSetup": "/DocumentTemplate/Home/Index#!DT/columnSettings"
                },
                {
                    "formName": "Cash Bank Setup",
                    "preFormCode": "00",
                    "groupSkuFlag": "I",
                    "iconPath": "fa fa-bank",
                    "urlForSetup": "/DocumentTemplate/Home/Index#!DT/cashBankSetup"
                },
                {
                    "formName": "Bank Reconcilation",
                    "preFormCode": "00",
                    "groupSkuFlag": "I",
                        "iconPath": "fa fa-sort-numeric-asc",
                    "urlForSetup": "/DocumentTemplate/Home/Index#!DT/bankReconcilation"
                },
                {
                    "formName": "Bank Gurantee",
                    "preFormCode": "00",
                    "groupSkuFlag": "I",
                    "iconPath": "fa fa-credit-card",
                    "urlForSetup": "/DocumentTemplate/Home/Index#!DT/bankGurantee"
                }
                ]
            });
           
            
            $scope.menuList[res.data.length] = {
                Items: [
                    {
                        formName: "Check(All)",
                        Level: 0,
                        formId: null,
                        groupSkuFlag: "G",
                        hasForms: false,
                        iconPath: null,
                        masterFormCode: "",
                        moduleCode: "04",
                        preFormCode: "00",
                        //urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/04"
                        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/All_Check"
                    },
                    {
                        formName: "UnCheck(All)",
                        Level: 0,
                        formId: null,
                        groupSkuFlag: "G",
                        hasForms: false,
                        iconPath: null,
                        masterFormCode: "",
                        moduleCode: "04",
                        preFormCode: "00",
                        //urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/04"
                        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/All_UnCheck"
                    },
                    {
                        formName: "Authorize(All)",
                        Level: 0,
                        formId: null,
                        groupSkuFlag: "G",
                        hasForms: false,
                        iconPath: null,
                        masterFormCode: "",
                        moduleCode: "04",
                        preFormCode: "00",
                        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/All_Authorise"
                        //urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/05"
                    },
                    {
                        formName: "UnAuthorize(All)",
                        Level: 0,
                        formId: null,
                        groupSkuFlag: "G",
                        hasForms: false,
                        iconPath: null,
                        masterFormCode: "",
                        moduleCode: "04",
                        preFormCode: "00",
                        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/All_UnAuthorise"
                        //urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/05"
                    },
                    {
                        formName: "Post(All)",
                        Level: 0,
                        formId: null,
                        groupSkuFlag: "G",
                        hasForms: false,
                        iconPath: null,
                        masterFormCode: "",
                        moduleCode: "04",
                        preFormCode: "00",
                        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/All_Post"
                        //urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/06"
                    },
                    {
                        formName: "UnPost(All)",
                        Level: 0,
                        formId: null,
                        groupSkuFlag: "G",
                        hasForms: false,
                        iconPath: null,
                        masterFormCode: "",
                        moduleCode: "04",
                        preFormCode: "00",
                        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/All_UnPost"
                        //urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/06"
                    }
                ],
                formName: "Verification",
                Level: 0,
                formId: null,
                groupSkuFlag: "G",
                hasForms: true,
                iconPath: null,
                masterFormCode: "",
                moduleCode: "00",
                preFormCode: "111111",
                urlForSetup: null
            };
            $.each($scope.menuList, function (id, val) {
                debugger;
                if (val.formName === "Financial Accounting") {
                    val.iconPath = generatRandomIcon();
                    $.each(val.Items, function (id1, val1) {
                        val1.iconPath = generatRandomIcon();
                        $.each(val1.Items, function (id2, val2) {
                            val2.iconPath = generatRandomIcon();
                            $.each(val2.Items, function (id3, val3) {
                                val3.iconPath = generatRandomIcon();
                            })
                        })
                    })
                }
                if (val.formName === "Inventory N Purcurement") {
                    val.iconPath = generatRandomIcon();
                    $.each(val.Items, function (id11, val11) {
                        val11.iconPath = generatRandomIcon();
                        $.each(val11.Items, function (id22, val22) {
                            val22.iconPath = generatRandomIcon();
                            $.each(val22.Items, function (id33, val33) {
                                val33.iconPath = generatRandomIcon();
                            })
                        })
                    })
                }
                if (val.formName === "Production Management") {
                    val.iconPath = generatRandomIcon();
                    $.each(val.Items, function (id111, val111) {
                        val111.iconPath = generatRandomIcon();
                        $.each(val111.Items, function (id222, val222) {
                            val222.iconPath = generatRandomIcon();
                            //$.each(val222.Items, function (id333, val333) {
                            //    val333.iconPath = generatRandomIcon();
                            //})
                        })
                    })
                }
                if (val.formName === "Sales N Revenue") {
                    val.iconPath = generatRandomIcon();
                    $.each(val.Items, function (id1111, val1111) {

                        // old submenu for Sales Verification
                        // old submenu for Sales Verification
                        //val1111.iconPath = generatRandomIcon();
                        //    val1111.Items[4] = {
                        //        formName: "Sales Check",
                        //        Level: 0,
                        //        formId: null,
                        //        groupSkuFlag: "G",
                        //        hasForms: false,
                        //        iconPath: null,
                        //        masterFormCode: "",
                        //        moduleCode: "04",
                        //        preFormCode: "00",
                        //        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/04"
                        //    };
                        //    val1111.Items[5] = {
                        //        formName: "Sales Authorize",
                        //        Level: 0,
                        //        formId: null,
                        //        groupSkuFlag: "G",
                        //        hasForms: false,
                        //        iconPath: null,
                        //        masterFormCode: "",
                        //        moduleCode: "04",
                        //        preFormCode: "00",
                        //        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/05"
                        //    };
                        //    val1111.Items[6] = {
                        //        formName: "Sales Post",
                        //        Level: 0,
                        //        formId: null,
                        //        groupSkuFlag: "G",
                        //        hasForms: false,
                        //        iconPath: null,
                        //        masterFormCode: "",
                        //        moduleCode: "04",
                        //        preFormCode: "00",
                        //        urlForSetup: "/DocumentTemplate/Template/SplitterIndex#!DT/VerifySplitter/06"
                        //    };                        
                        $.each(val1111.Items, function (id2222, val2222) {
                            val2222.iconPath = generatRandomIcon();
                            //$.each(val2222.Items, function (id3333, val3333) {
                            //    val3333.iconPath = generatRandomIcon();
                            //})
                        })
                    })
                }

                if (val.formName === "LC") {
                    val.iconPath = generatRandomIcon();
                }
                if (val.formName === "Planning") {
                    val.iconPath = generatRandomIcon();
                }
                if (val.formName === "Business intelligence Tool") {
                    val.iconPath = generatRandomIcon();
                }
                if (val.formName === "Verification") {                    
                    val.iconPath = generatRandomIcon();
                    $.each(val.Items, function (id1111, val1111) {
                        val1111.iconPath = generatRandomIcon();
                    });
                }
            })
        });
        menuService.getInventoryMenuDetail().then(function (d) {
            $scope.InventoryMenuData = d.data;
            $.each($scope.InventoryMenuData, function (id, val) {
                val["COLOR"] = getRandomColor();
                val["ICON_PATH"] = generatRandomIcon();
            });
        }, function () {
            alert("Error at: Get All Menu Items")
        });
        menuService.getAllSetupMenuDetail().then(function (d) {
            $scope.SetupMenuData = d.data;

        }, function () {
            alert("Error at: Get All Set up Menu Items")
        });

        menuService.getSalesMenuDetail().then(function (d) {

            $scope.SalesMenuData = d.data;
            $.each($scope.SalesMenuData, function (id, val) {
                val["COLOR"] = getRandomColor();
                val["ICON_PATH"] = generatRandomIcon();
            });
        }, function () {
            alert("Error at: Get All Menu Items")
        });

        function isEmpty(obj) {
            return Object.keys(obj).length == 0;
        }

        menuService.InventoryAssigneeDraftList().then(function (d) {
            $scope.InventoryDraftData = d.data;
            $.each($scope.InventoryDraftData, function (id, val) {

                val["COLOR"] = getRandomColor();
                val["ICON_PATH"] = generatRandomIcon();
                val["ICON_SC"] = generateformshortcut(val.FORM_EDESC);

            });
        }, function () {
            alert("Error at: Get All Menu Items")
        });

        menuService.InventoryAssigneeSavedDraftList().then(function (d) {
            $scope.InventorySavedDraftData = d.data;
            $.each($scope.InventorySavedDraftData, function (id, val) {

                val["COLOR"] = getRandomColor();
                val["ICON_PATH"] = generatRandomIcon();
                val["ICON_SC"] = generateformshortcut(val.FORM_EDESC);
            });
        }, function () {
            alert("Error at: Get All Menu Items")
        });
    };



    var getFormsUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetFormsTree";
    $scope.formTreeData = new kendo.data.HierarchicalDataSource({

        transport: {
            read: {
                url: getFormsUrl,
                type: 'GET',
                data: function (data, evt) {
                }
            },

        },
        schema: {
            parse: function (data) {
                return data;
            },
            model: {
                id: "MASTER_FORM_CODE",
                parentId: "PRE_FORM_CODE",
                children: "Items",

                fields: {
                    FORM_CODE: { field: "FORM_CODE", type: "string" },
                    FORM_EDESC: { field: "FORM_EDESC", type: "string" },
                    parentId: { field: "PRE_FORM_CODE", type: "string", defaultValue: "111111" },
                }
            }
        }
    });

    $scope.databound = function () {

        var treeview = $("#formTreeView").data("kendoTreeView");
        treeview.expand("> .k-group > .k-item");

    }

    $scope.formTreeOnSelect = function (dataItem) {

        showloader();
        if (dataItem.formName == "Organizer") {
            $window.location.href = '/DocumentTemplate/Home/Dashboard';
        }
        else if (dataItem.groupSkuFlag == "I") {
            if (dataItem.moduleCode == "04") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/formtemplate/' + dataItem.formId;
            }
            else if (dataItem.moduleCode == "01") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/FinanceVoucher?formcode=' + dataItem.formId;
            }
            else if (dataItem.moduleCode == "02") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/Inventory?formcode=' + dataItem.formId;
            }
            else if (dataItem.moduleCode == "03") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/ProdManagement?formcode=' + dataItem.formId;
            }
        }
        else if (dataItem.moduleCode == "setup") {
            $window.location.href = dataItem.urlForSetup;
        }
        hideloader();
    }

    var tooltip = $("#example").kendoTooltip({
        filter: "li span.k-in",
        content: "Loading...",
        position: "right",
        width: 200,
        show: function (e) {

            var treeview = $("#formTreeView").data("kendoTreeView");
            var tooltip = this;
            var text = treeview.text(tooltip.target());
            tooltip.content.text(tooltip.target()[0].innerText);
        }
    }).data("kendoTooltip");


    $scope.onContextSelect = function (e) {

        var button = $(e.item);
        var node = $(e.target);
        var dataItem = $("#formTreeView").data("kendoTreeView").dataItem(node);
        if ($(e.item).text().trim() == "New") {
            $scope.formTreeOnSelect(dataItem);
        }
        else if ($(e.item).text().trim() == "Document Finder") {
            $window.location.href = '/DocumentTemplate/Template/SplitterIndex#!DT/MenuSplitter/' + dataItem.moduleCode;
        }
        else if ($(e.item).text().trim() == "List of Draft") {
            var draft = "draft";
            if (dataItem.moduleCode == "04") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/formtemplate/' + dataItem.formId + "?menu=" + draft;
            }
            else if (dataItem.moduleCode == "01") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/FinanceVoucher?formcode=' + dataItem.formId + "&menu=" + draft;
            }
            else if (dataItem.moduleCode == "02") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/Inventory?formcode=' + dataItem.formId + "&menu=" + draft;
            }
            else if (dataItem.moduleCode == "03") {
                $window.location.href = '/DocumentTemplate/Home/Index#!DT/ProdManagement?formcode=' + dataItem.formId + "&menu=" + draft;
            }

        }

    };
    $scope.contextMenuOpen = function (e) {

        var button = $(e.item);
        var node = $(e.target);
        var dataItem = $("#formTreeView").data("kendoTreeView").dataItem(node);
        if (dataItem.groupSkuFlag == "G") {
            e.preventDefault();
        }
    };

    //var getRandomColor = function () {
    //    return '#' +Math.floor(Math.random() * 16777215).toString(16)
    //};
    function getRandomColor() {
        var letters = '0123456789ABCDEF'.split('');
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.round(Math.random() * 10)];
        }
        return color;
    }

    $scope.getMenuItems();

    var iconArray = new Array('fa fa-search', 'fa fa-envelope-o', 'fa fa-heart', 'fa fa-star', 'fa fa-star-o', 'fa fa-user', 'fa fa-film', 'fa fa-th-large', 'fa fa-th',
        'fa fa-th-list', 'fa fa-check', 'fa fa-times', 'fa fa-search-plus', 'fa fa-search-minus', 'fa fa-power-off', 'fa fa-signal', 'fa fa-cog', 'fa fa-trash-o', 'fa fa-home', 'fa fa-file-o', 'fa fa-clock-o',
        'fa fa-road', 'fa fa-download', 'fa fa-arrow-circle-o-down', 'fa fa-arrow-circle-o-up', 'fa fa-inbox', 'fa fa-play-circle-o', 'fa fa-repeat', 'fa fa-refresh', 'fa fa-list-alt', 'fa fa-lock', 'fa fa-flag',
        'fa fa-headphones', 'fa fa-volume-off', 'fa fa-volume-down', 'fa fa-volume-up', 'fa fa-qrcode', 'fa fa-barcode', 'fa fa-tag', 'fa fa-tags', 'fa fa-book', 'fa fa-bookmark', 'fa fa-print', 'fa fa-camera',
        'fa fa-font', 'fa fa-bold', 'fa fa-italic', 'fa fa-text-height', 'fa fa-text-width', 'fa fa-align-left', 'fa fa-align-center', 'fa fa-align-right', 'fa fa-align-justify', 'fa fa-list', 'fa fa-outdent',
        'fa fa-indent', 'fa fa-video-camera', 'fa fa-picture-o', 'fa fa-pencil', 'fa fa-map-marker', 'fa fa-adjust', 'fa fa-tint', 'fa fa-pencil-square-o', 'fa fa-share-square-o', 'fa fa-check-square-o',
        'fa fa-arrows', 'fa fa-step-backward', 'fa fa-fast-backward', 'fa fa-backward', 'fa fa-play', 'fa fa-pause', 'fa fa-stop', 'fa fa-forward', 'fa fa-fast-forward', 'fa fa-step-forward', 'fa fa-eject',
        'fa fa-chevron-left', 'fa fa-chevron-right', 'fa fa-plus-circle', 'fa fa-minus-circle', 'fa fa-times-circle', 'fa fa-check-circle', 'fa fa-question-circle', 'fa fa-info-circle', 'fa fa-crosshairs',
        'fa fa-times-circle-o', 'fa fa-check-circle-o', 'fa fa-ban', 'fa fa-arrow-left', 'fa fa-arrow-right', 'fa fa-arrow-up', 'fa fa-arrow-down', 'fa fa-share', 'fa fa-expand', 'fa fa-compress', 'fa fa-plus',
        'fa fa-minus', 'fa fa-asterisk', 'fa fa-exclamation-circle', 'fa fa-gift', 'fa fa-leaf', 'fa fa-fire', 'fa fa-eye', 'fa fa-eye-slash', 'fa fa-exclamation-triangle', 'fa fa-plane', 'fa fa-calendar', 'fa fa-random',
        'fa fa-comment', 'fa fa-magnet', 'fa fa-chevron-up', 'fa fa-chevron-down', 'fa fa-retweet', 'fa fa-shopping-cart', 'fa fa-folder', 'fa fa-folder-open', 'fa fa-arrows-v', 'fa fa-arrows-h', 'fa fa-bar-chart', 
        'fa fa-camera-retro', 'fa fa-key', 'fa fa-cogs', 'fa fa-comments', 'fa fa-thumbs-o-up', 'fa fa-thumbs-o-down', 'fa fa-star-half', 'fa fa-heart-o', 'fa fa-thumb-tack', 'fa fa-external-link', 'fa fa-sign-in', 'fa fa-trophy', 'fa fa-github-square', 'fa fa-upload', 'fa fa-lemon-o',
        'fa fa-phone', 'fa fa-square-o', 'fa fa-bookmark-o', 'fa fa-phone-square', 'fa fa-unlock', 'fa fa-credit-card', 'fa fa-hdd-o', 'fa fa-bullhorn', 'fa fa-bell', 'fa fa-certificate', 'fa fa-hand-o-right', 'fa fa-hand-o-left', 'fa fa-hand-o-up',
        'fa fa-hand-o-down',
        'fa fa-arrow-circle-left',
        'fa fa-arrow-circle-right',
        'fa fa-arrow-circle-up',
        'fa fa-arrow-circle-down',
        'fa fa-globe',
        'fa fa-wrench',
        'fa fa-tasks',
        'fa fa-filter',
        'fa fa-briefcase',
        'fa fa-arrows-alt',
        'fa fa-users',
        'fa fa-link',
        'fa fa-cloud',
        'fa fa-flask',
        'fa fa-scissors',
        'fa fa-files-o',
        'fa fa-paperclip',
        'fa fa-floppy-o',
        'fa fa-square',
        'fa fa-bars',
        'fa fa-list-ul',
        'fa fa-list-ol',
        'fa fa-strikethrough',
        'fa fa-underline',
        'fa fa-table',
        'fa fa-magic',
        'fa fa-money',
        'fa fa-caret-down',
        'fa fa-caret-up',
        'fa fa-caret-left',
        'fa fa-caret-right',
        'fa fa-columns',
        'fa fa-sort',
        'fa fa-sort-desc',
        'fa fa-sort-asc',
        'fa fa-envelope',
        'fa fa-undo',
        'fa fa-gavel',
        'fa fa-tachometer',
        'fa fa-comment-o',
        'fa fa-comments-o',
        'fa fa-bolt',
        'fa fa-sitemap',
        'fa fa-umbrella',
        'fa fa-clipboard',
        'fa fa-lightbulb-o',
        'fa fa-exchange',
        'fa fa-cloud-download',
        'fa fa-cloud-upload',
        'fa fa-user-md',
        'fa fa-stethoscope',
        'fa fa-suitcase',
        'fa fa-bell-o',
        'fa fa-coffee',
        'fa fa-cutlery',
        'fa fa-file-text-o',
        'fa fa-building-o',
        'fa fa-hospital-o',
        'fa fa-ambulance',
        'fa fa-medkit',
        'fa fa-fighter-jet',
        'fa fa-beer',
        'fa fa-h-square',
        'fa fa-plus-square',
        'fa fa-angle-double-left',
        'fa fa-angle-double-right',
        'fa fa-angle-double-up',
        'fa fa-angle-double-down',
        'fa fa-angle-left',
        'fa fa-angle-right',
        'fa fa-angle-up',
        'fa fa-angle-down',
        'fa fa-desktop',
        'fa fa-laptop',
        'fa fa-tablet',
        'fa fa-mobile',
        'fa fa-circle-o',
        'fa fa-quote-left',
        'fa fa-quote-right',
        'fa fa-spinner',
        'fa fa-circle',
        'fa fa-reply',
        'fa fa-github-alt',
        'fa fa-folder-o',
        'fa fa-folder-open-o',
        'fa fa-smile-o',
        'fa fa-frown-o',
        'fa fa-meh-o',
        'fa fa-gamepad',
        'fa fa-keyboard-o',
        'fa fa-flag-o',
        'fa fa-flag-checkered',
        'fa fa-terminal',
        'fa fa-code',
        'fa fa-reply-all',
        'fa fa-star-half-o',
        'fa fa-location-arrow',
        'fa fa-crop',
        'fa fa-code-fork',
        'fa fa-chain-broken',
        'fa fa-question',
        'fa fa-info',
        'fa fa-exclamation',
        'fa fa-superscript',
        'fa fa-subscript',
        'fa fa-eraser',
        'fa fa-puzzle-piece',
        'fa fa-microphone',
        'fa fa-microphone-slash',
        'fa fa-shield',
        'fa fa-calendar-o',
        'fa fa-fire-extinguisher',
        'fa fa-rocket',
        'fa fa-maxcdn',
        'fa fa-chevron-circle-left',
        'fa fa-chevron-circle-right',
        'fa fa-chevron-circle-up',
        'fa fa-chevron-circle-down',
        'fa fa-html5',
        'fa fa-css3',
        'fa fa-anchor',
        'fa fa-unlock-alt',
        'fa fa-bullseye',
        'fa fa-ellipsis-h',
        'fa fa-ellipsis-v',
        'fa fa-play-circle',
        'fa fa-ticket',
        'fa fa-minus-square',
        'fa fa-minus-square-o',
        'fa fa-level-up',
        'fa fa-level-down',
        'fa fa-check-square',
        'fa fa-pencil-square',
        'fa fa-share-square',
        'fa fa-compass',
        'fa fa-caret-square-o-down',
        'fa fa-caret-square-o-up',
        'fa fa-caret-square-o-right',
        'fa fa-eur',
        'fa fa-gbp',
        'fa fa-usd',
        'fa fa-inr',
        'fa fa-jpy',
        'fa fa-rub',
        'fa fa-krw',
        'fa fa-btc',
        'fa fa-file',
        'fa fa-file-text',
        'fa fa-sort-alpha-asc',
        'fa fa-sort-alpha-desc',
        'fa fa-sort-amount-asc',
        'fa fa-sort-amount-desc',
        'fa fa-sort-numeric-asc',
        'fa fa-sort-numeric-desc',
        'fa fa-thumbs-up',
        'fa fa-thumbs-down',
        'fa fa-long-arrow-down',
        'fa fa-long-arrow-up',
        'fa fa-long-arrow-left',
        'fa fa-long-arrow-right',
        'fa fa-foursquare',
        'fa fa-trello',
        'fa fa-female',
        'fa fa-male',
        'fa fa-gratipay',
        'fa fa-sun-o',
        'fa fa-moon-o',
        'fa fa-archive',
        'fa fa-bug',
        'fa fa-vk',
        'fa fa-weibo',
        'fa fa-renren',
        'fa fa-pagelines',
        'fa fa-stack-exchange',
        'fa fa-arrow-circle-o-right',
        'fa fa-arrow-circle-o-left',
        'fa fa-caret-square-o-left',
        'fa fa-dot-circle-o',
        'fa fa-wheelchair',
        'fa fa-try',
        'fa fa-plus-square-o',
        'fa fa-space-shuttle',
        'fa fa-slack',
        'fa fa-envelope-square',
        'fa fa-wordpress',
        'fa fa-openid',
        'fa fa-university',
        'fa fa-graduation-cap',
        'fa fa-reddit-square',
        'fa fa-stumbleupon-circle',
        'fa fa-stumbleupon',
        'fa fa-delicious',
        'fa fa-digg',
        'fa fa-pied-piper-pp',
        'fa fa-pied-piper-alt',
        'fa fa-language',
        'fa fa-fax',
        'fa fa-building',
        'fa fa-child',
        'fa fa-paw',
        'fa fa-spoon',
        'fa fa-cube',
        'fa fa-cubes',
        'fa fa-behance',
        'fa fa-behance-square',
        'fa fa-steam',
        'fa fa-steam-square',
        'fa fa-recycle',
        'fa fa-car',
        'fa fa-taxi',
        'fa fa-tree',
        'fa fa-spotify',
        'fa fa-deviantart',
        'fa fa-soundcloud',
        'fa fa-database',
        'fa fa-file-pdf-o',
        'fa fa-file-word-o',
        'fa fa-file-excel-o',
        'fa fa-file-powerpoint-o',
        'fa fa-file-image-o',
        'fa fa-file-archive-o',
        'fa fa-file-audio-o',
        'fa fa-file-video-o',
        'fa fa-file-code-o',
        'fa fa-vine',
        'fa fa-codepen',
        'fa fa-jsfiddle',
        'fa fa-life-ring',
        'fa fa-circle-o-notch',
        'fa fa-rebel',
        'fa fa-empire',
        'fa fa-git-square',
        'fa fa-git',
        'fa fa-hacker-news',
        'fa fa-tencent-weibo',
        'fa fa-qq',
        'fa fa-weixin',
        'fa fa-paper-plane',
        'fa fa-paper-plane-o',
        'fa fa-history',
        'fa fa-circle-thin',
        'fa fa-header',
        'fa fa-paragraph',
        'fa fa-sliders',
        'fa fa-share-alt',
        'fa fa-share-alt-square',
        'fa fa-bomb',
        'fa fa-futbol-o',
        'fa fa-tty',
        'fa fa-binoculars',
        'fa fa-plug',
        'fa fa-slideshare',
        'fa fa-twitch',
        'fa fa-yelp',
        'fa fa-newspaper-o',
        'fa fa-calculator',
        'fa fa-cc-stripe',
        'fa fa-bell-slash',
        'fa fa-bell-slash-o',
        'fa fa-trash',
        'fa fa-copyright',
        'fa fa-at',
        'fa fa-eyedropper',
        'fa fa-paint-brush',
        'fa fa-birthday-cake',
        'fa fa-area-chart',
        'fa fa-pie-chart',
        'fa fa-line-chart',
        'fa fa-lastfm',
        'fa fa-lastfm-square',
        'fa fa-toggle-off',
        'fa fa-toggle-on',
        'fa fa-bicycle',
        'fa fa-bus',
        'fa fa-ioxhost',
        'fa fa-angellist',
        'fa fa-cc',
        'fa fa-ils',
        'fa fa-meanpath',
        'fa fa-buysellads',
        'fa fa-connectdevelop',
        'fa fa-dashcube',
        'fa fa-forumbee',
        'fa fa-leanpub',
        'fa fa-sellsy',
        'fa fa-shirtsinbulk',
        'fa fa-simplybuilt',
        'fa fa-skyatlas',
        'fa fa-cart-plus',
        'fa fa-cart-arrow-down',
        'fa fa-diamond',
        'fa fa-ship',
        'fa fa-user-secret',
        'fa fa-motorcycle',
        'fa fa-street-view',
        'fa fa-heartbeat',
        'fa fa-venus',
        'fa fa-mars', 'fa fa-mercury', 'fa fa-transgender', 'fa fa-transgender-alt', 'fa fa-venus-double', 'fa fa-mars-double', 'fa fa-venus-mars', 'fa fa-mars-stroke', 'fa fa-mars-stroke-v',
        'fa fa-mars-stroke-h', 'fa fa-neuter', 'fa fa-genderless', 'fa fa-server',
        'fa fa-user-plus',
        'fa fa-user-times',
        'fa fa-bed',
        'fa fa-viacoin',
        'fa fa-train',
        'fa fa-subway',
        'fa fa-medium',
        'fa fa-y-combinator',
        'fa fa-optin-monster',
        'fa fa-expeditedssl',
        'fa fa-battery-full',
        'fa fa-battery-three-quarters',
        'fa fa-battery-half',
        'fa fa-battery-quarter',
        'fa fa-battery-empty',
        'fa fa-mouse-pointer',
        'fa fa-i-cursor',
        'fa fa-object-group',
        'fa fa-object-ungroup',
        'fa fa-sticky-note',
        'fa fa-sticky-note-o',
        'fa fa-cc-jcb',
        'fa fa-cc-diners-club',
        'fa fa-clone',
        'fa fa-balance-scale',
        'fa fa-hourglass-o',
        'fa fa-hourglass-start',
        'fa fa-hourglass-half',
        'fa fa-hourglass-end',
        'fa fa-hourglass',
        'fa fa-hand-rock-o',
        'fa fa-hand-paper-o',
        'fa fa-hand-scissors-o',
        'fa fa-hand-lizard-o',
        'fa fa-hand-spock-o',
        'fa fa-hand-pointer-o',
        'fa fa-hand-peace-o',
        'fa fa-trademark',
        'fa fa-registered',
        'fa fa-creative-commons',
        'fa fa-gg',
        'fa fa-gg-circle',
        'fa fa-television',
        'fa fa-contao',
        'fa fa-500px',
        'fa fa-calendar-plus-o',
        'fa fa-calendar-minus-o',
        'fa fa-calendar-times-o',
        'fa fa-calendar-check-o',
        'fa fa-industry',
        'fa fa-map-pin',
        'fa fa-map-signs',
        'fa fa-map-o',
        'fa fa-map',
        'fa fa-commenting',
        'fa fa-commenting-o',
        'fa fa-houzz',
        'fa fa-black-tie',
        'fa fa-fonticons',
        'fa fa-reddit-alien',
        'fa fa-edge',
        'fa fa-credit-card-alt',
        'fa fa-codiepie',
        'fa fa-modx',
        'fa fa-fort-awesome',
        'fa fa-usb',
        'fa fa-product-hunt',
        'fa fa-mixcloud',
        'fa fa-scribd',
        'fa fa-pause-circle',
        'fa fa-pause-circle-o',
        'fa fa-stop-circle',
        'fa fa-stop-circle-o',
        'fa fa-shopping-bag',
        'fa fa-shopping-basket',
        'fa fa-hashtag',
        'fa fa-percent',
        'fa fa-wpbeginner',
        'fa fa-wpforms',
        'fa fa-envira',
        'fa fa-universal-access',
        'fa fa-wheelchair-alt',
        'fa fa-question-circle-o',
        'fa fa-blind',
        'fa fa-audio-description',
        'fa fa-volume-control-phone',
        'fa fa-braille',
        'fa fa-assistive-listening-systems',
        'fa fa-american-sign-language-interpreting',
        'fa fa-deaf',
        'fa fa-glide',
        'fa fa-glide-g',
        'fa fa-sign-language',
        'fa fa-low-vision',
        'fa fa-viadeo',
        'fa fa-viadeo-square',
        'fa fa-pied-piper',
        'fa fa-first-order',
        'fa fa-yoast',
        'fa fa-themeisle',
        'fa fa-font-awesome');

    $scope.iconArray = [];
    function SetIcon() {

        iconArray;
        angular.forEach(iconArray, function (value, key) {
            $scope.iconArray.push({ icon: value });
        });
        $scope.iconArray;
    }
    SetIcon();
    $scope.Bind = function () {
        $('.k-list').slimScroll({
            height: '250px'
        });
    };

    $scope.iconlist = {
        optionLabel: "--Select Icon--",
        dataTextField: "icon",
        dataValueField: "icon",
        dataSource: $scope.iconArray,
        filter: "contains",
        template: '<span class="k-state-default"> <i class="{{dataItem.icon}}"></i> {{dataItem.icon}}</span>',
        dataBound: function (e) {
            $scope.Bind();
        }
    };


   
    
    $scope.ColorArray = [
    { text: "Green", value: "#1f9400" },
    { text: "Purple", value: "#716aca" },
    { text: "Red", value: "#d6213c" },
    { text: "Yellow", value: "#ffb822" },
    { text: "Blue", value: "#0e5293" },
    {text:'Grey',value:'#a7a1a1'},
    {text:'Drak Blue',value:'#3623af'}
    ];



    $scope.ColorList = {
        optionLabel: "--Select Color--",
        dataTextField: "text",
        dataValueField: "value",
        dataSource: $scope.ColorArray,
        filter: "contains",
        template: '<span class="k-state-default"> {{dataItem.text}} </span><span style="background-color:{{dataItem.text}}; border-style:solid; border-width:thin; padding:0px 10px 1px 10px;display: inline-grid; height:13px;float:right;margin-left: 20px;"></span>',
        dataBound: function (e) {
            $scope.Bind();
        }
    };


    var generatRandomIcon = function () {
        var randIcon = Math.floor(Math.random() * (iconArray.length));
        return iconArray[randIcon];
    }
    var generateformshortcut = function (e) {
        var arr = e.split(' ').slice(0, 2);
        var shortcut = '';
        $.each(arr, function (key, value) {

            shortcut = shortcut + value.charAt(0);
        });

        return shortcut;

    }

    $scope.colors = [
        '#C66', '#C93', '#F66', '#36C', '#C96', '#633', '#069', '#F90', '#6C3', '#666', '#a136c7', '#2583ce', '#da2c2c',
        '#00b4ac', '#009dd8', '#25b846', '#da2c2c', '#f27022', '#6438c8', '#fbbc11'
    ];

    $scope.randomColor = function (parentIndex, index, parentLast, elementLast) {
        //$($($(".erp-iconlist")[parentIndex]).find("ul").first().find("li")[index]).find(".circle").css('background-color', _.shuffle($scope.colors)[0]);           
        if (parentLast && elementLast) {

            $('.ajax-loading').hide();
            $('[data-toggle="tooltip"]').tooltip();
            $(".icondescription").on("click", "span", function (e) {
                e.preventDefault();
                $(".icondescription span").hide();
                bootbox.confirm({
                    message: "Do you want to delete this menu? This cannot be undone.",
                    buttons: {
                        cancel: {
                            label: 'Cancel'
                        },
                        confirm: {
                            label: 'Confirm'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            var value = $(e.currentTarget).attr("data");
                            $.ajax({
                                type: 'POST',
                                dataType: "json",
                                url: window.location.protocol + "//" + window.location.host + "/Home/DeleteFavouriteMenu?menuName=" + value,
                                success: function (data) {
                                    if (data == 200) {
                                        $("a[data-name='" + value + "']").parent().remove();
                                        displayPopupNotification("Menu Deleted Successfully", "success");
                                    }
                                    else {
                                        displayPopupNotification("Error", "error");
                                    }
                                }
                            });
                        }

                    }
                });

            });
        }
        if (parentLast && _.isEmpty(elementLast)) {
            $('.ajax-loading').hide();
        }
    }

    $scope.modelABBRColor = function (modelCode) {
        if (modelCode == 'SA')
            return "#43a12e";
        else if (modelCode == 'AC')
            return "#3c763d";
        else if (modelCode == 'AR')
            return "#4480a4";
        else if (modelCode == 'PR')
            return "#666";
        else if (modelCode == 'ST')
            return "#31708f";
        else if (modelCode == 'FA')
            return "#5aa9d7";
        else if (modelCode == 'NA')
            return "#45b6b6";
        else
            return "#b6a845";
    }
    //inventory draft


    ////finance voucher draft
    //menuService.FinanceVoucherAssigneeDraftList().then(function (d) {
    //    $scope.FinanceVoucherDraftData = d.data;
    //    $.each($scope.FinanceVoucherDraftData, function (id, val) {
    //        val["COLOR"] = getRandomColor();
    //        val["ICON_PATH"] = generatRandomIcon();
    //    });
    //}, function () {
    //    alert("Error at: Get All Menu Items")
    //});

    //menuService.FinanceVoucherAssigneeSavedDraftList().then(function (d) {
    //    $scope.FinanceVoucherSavedDraftData = d.data;
    //    $.each($scope.FinanceVoucherSavedDraftData, function (id, val) {
    //        val["COLOR"] = getRandomColor();
    //        val["ICON_PATH"] = generatRandomIcon();
    //    });
    //}, function () {
    //    alert("Error at: Get All Menu Items")
    //});

    ////finance voucher draft
    //menuService.SalesAssigneeDraftList().then(function (d) {
    //    $scope.SalesDraftData = d.data;
    //    $.each($scope.SalesDraftData, function (id, val) {
    //        val["COLOR"] = getRandomColor();
    //        val["ICON_PATH"] = generatRandomIcon();
    //    });
    //}, function () {
    //    alert("Error at: Get All Menu Items")
    //});

    //menuService.SalesAssigneeSavedDraftList().then(function (d) {
    //    $scope.SalesSavedDraftData = d.data;
    //    $.each($scope.SalesSavedDraftData, function (id, val) {
    //        val["COLOR"] = getRandomColor();
    //        val["ICON_PATH"] = generatRandomIcon();
    //    });
    //}, function () {
    //    alert("Error at: Get All Menu Items")
    //});


    //draft start
    //$scope.GetDTData = function (formCode, tempCode) {
    //  
    //    window.location.href = "/DocumentTemplate/Home/Index#!DT/Draft/" + formCode + "/" + tempCode + "";
    //};




    $scope.AddFolder = function () {
        
        var FOLDER = $scope.FOLDER_NAME;
        var FOLDER_COLOR = $scope.FOLDER_COLOR;
        FOLDER_COLOR = FOLDER_COLOR.split("#").slice(1, 5)[0];
        var ICON = $scope.ICON;
        var req = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/AddNewFolder?FOLDER=" + FOLDER + "&FOLDER_COLOR=" + FOLDER_COLOR + "&ICON=" + ICON;
        $http.get(req).then(function (result) {
            
            $("#ContentModal").modal("hide");
            window.location.reload();

        });
    };

    $scope.CancelFolder = function () {
        $scope.FOLDER_NAME = "";
        $scope.FONT_COLOR = "";
        $scope.ICON = "";
    }

    $scope.GetInvDTData = function (formCode, tempCode) {

        window.location.href = "/DocumentTemplate/Home/Index#!DT/DraftInventory/" + formCode + "/" + tempCode + "";
    };
    $scope.GetFVDTData = function (formCode, tempCode) {

        window.location.href = "/DocumentTemplate/Home/Index#!DT/DraftFinanceVoucher/" + formCode + "/" + tempCode + "";
    };
    $scope.GetSalesDTData = function (formCode, tempCode) {

        window.location.href = "/DocumentTemplate/Home/Index#!DT/DraftFormtemplate/" + formCode + "/" + tempCode + "";
    };

    window.menutarget = "";
    $scope.initMenu = function (targetid) {
            target = ("#" + targetid).trim();
            window.menutarget = target;
            menu = $("#sidebar-context-menu").kendoContextMenu({
            orientation: "vertical",
            target: window.menutarget,
            animation: {
                open: { effects: "fadeIn" },
                duration: 500
            },
            select: function (e) {
                // Do something more complicated on select
            }
        });
    };
 

   

    $scope.Menuclick = function (event) {
        
        var id = event.currentTarget.id;
        switch (event.which) {
          case 3:
                $scope.initMenu(id);
                break;
            default:
              
        }
    };

  

    $scope.ShowFolder = function (search) {
        

        search = search.toUpperCase();
        var len = $(".Dragtitle").length;
        for (var i = 0; i < len; i++) {
            var text = $($(".Dragtitle")[i]).text().toUpperCase();
            var find = false;
            if (text.indexOf(search) > -1) {
                find = true;
            } else {
                find = false;
            }
            if (find) {
                $($($($($(".Dragtitle")[i]).parent()[0]).parent()[0]).parent()[0]).css("display", "block");
            } else {
                $($($($($(".Dragtitle")[i]).parent()[0]).parent()[0]).parent()[0]).css("display", "none");
            }
        }

    }

    //$scope.ShowIcon = function (searchBox) {
    //    
    //     search = searchBox.toUpperCase();
    //    var len = $(".Folder_Icon").length;

    //    for (var i = 0; i < len; i++) {
    //        var text = $($($(".Folder_Icon").find("a")[0]).find("h6")[0]).text().trim().toUpperCase();
    //        var find = false;
    //        if (text.indexOf(search) > -1) {
    //            
    //            find = true;
    //        } else {
    //            find = false;
    //        }
    //        if (find) {
    //            $(".Folder_Icon").css("display", "block");
    //        } else {
    //            $(".Folder_Icon").css("display", "none");
    //        }
    //    }
    //}


    $scope.ShowIcon = function (searchBox) {
        search = searchBox.toUpperCase();
        var noresult = 0;
        if (search == "") {
            $('.SearchIcon > li').show();
            noresult = 1;

        } else {
            $('.SearchIcon > li').each(function (index) {
                var text = $($('.SearchIcon > li > a > h6')[index]).text().toUpperCase();;
                var match = text.indexOf(search);
                if (match >= 0) {
                    $(this).show();
                    noresult = 1;
                } else {
                    $(this).hide();
                }
            });

        };

    };

    //draft end
});


DTModule.filter('randomize', function () {
    return function (input, scope) {
        
        if (input != null && input != undefined && input > 1) {
            return Math.floor((Math.random() * input) + 1);
        }
    }
});


DTModule.service('menuService', ['$http', '$q', function ($http, $q) {

    var fac = {};
    fac.getMenuDetail = function () {
        return $http.get('/api/TemplateApi/GetAllMenuItems');
    }
    fac.getDynamicMenus = function () {
        
        return $http.get('/api/TemplateApi/GetFormsTree');
    }
    fac.getInventoryMenuDetail = function () {
        return $http.get('/api/TemplateApi/GetAllInventoryMenuItems');
    }

    fac.getAllSetupMenuDetail = function () {
        return $http.get('/api/TemplateApi/GetAllSetupMenuItems');
    }
    fac.getSalesMenuDetail = function () {
        return $http.get('/api/TemplateApi/GetAllSalesMenuItems');
    }
    fac.InventoryAssigneeDraftList = function () {
        return $http.get('/api/TemplateApi/GetAllMenuInventoryAssigneeDraftTemplateList');
    }
    fac.InventoryAssigneeSavedDraftList = function () {
        return $http.get('/api/TemplateApi/GetAllMenuInventoryAssigneeSavedDraftTemplateList');
    }
    //folder api
    fac.FetchFolderList = function () {
        return $http.get('/api/TemplateApi/GetFoldertByUserId ');
    }


    //check imp
    //fac.FetchFolderTemplateList = function () {
    //    return $http.get('/api/TemplateApi/GetFolderTemplateByUserId ');
    //}

    fac.FetchFolderTemplateList = function () {
        return $http.get('/Template/GetFolderTemplateByUserId');
    }

    //fac.FinanceVoucherAssigneeDraftList = function () {
    //    return $http.get('/api/TemplateApi/GetAllMenuFinanceVoucherAssigneeDraftTemplateList');
    //}
    //fac.FinanceVoucherAssigneeSavedDraftList = function () {
    //    return $http.get('/api/TemplateApi/GetAllMenuFinanceVoucherAssigneeSavedDraftTemplateList');
    //}
    //fac.SalesAssigneeDraftList = function () {
    //    return $http.get('/api/TemplateApi/GetAllMenuSalesAssigneeDraftTemplateList');
    //}
    //fac.SalesAssigneeSavedDraftList = function () {
    //    return $http.get('/api/TemplateApi/GetAllMenuSalesAssigneeSavedDraftTemplateList');
    //}

    return fac;
}]);