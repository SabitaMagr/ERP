function ManualEmailSender(config) {
    var mythis;
    var _gridName = "grid";
    var _gridRefreshTriggerName = "RunQuery";
    var _reportName = "file";
    var _submitButtonName = "sendMail";
    var _submitFormName = "sendMailForm";
    var _fileUploaded = false;
    var _responseMessage = "responseMessage";
    var _emailFormField = {
        emailAddress: "Email",
        subject: "Subject",
        fileType: "FileType",
        message: "Message",
        fileName:"FileName"
    }

    var _saveUrl = Metronic.getGlobalUrl() + "api/Common/Save";
    var _messageSaveUrl = Metronic.getGlobalUrl() + "api/Common/SaveMessage";
    if (config != undefined && config != null) {
        _gridName = config.gridName != undefined? config.gridName : _gridName;
        _gridRefreshTriggerName = config.gridRefreshTriggerName!= undefined? config.gridRefreshTriggerName:_gridRefreshTriggerName;
        _reportName = config.reportName != undefined ? config.reportName : _reportName;
        _submitButtonName = config.submitButtonName != undefined ? config.submitButtonName : _submitButtonName;
        _submitFormName = config.submitFormName != undefined ? config.submitFormName : _submitFormName;
        _responseMessage = config.responseMessage != undefined ? config.responseMessage : _responseMessage;
        if(config.emailFormField != undefined)
        {
            var emailFields = config.emailFormField;
            _emailFormField.emailAddress = emailFields.emailAddress != undefined ? emailFields.emailAddress : _emailFormField.emailAddress;
            _emailFormField.subject = emailFields.subject != undefined ? emailFields.subject : _emailFormField.subject;
            _emailFormField.fileType = emailFields.fileType != undefined ? emailFields.fileType : _emailFormField.emailAddress;
            _emailFormField.message = emailFields.message != undefined ? emailFields.message : _emailFormField.message;
            _emailFormField.fileName = emailFields.fileName != undefined ? emailFields.fileName : _emailFormField.fileName;
        }
    }

    var generateFileName = function (fileExtension) {
        var date = new Date();
        return _reportName + "_" + date.getTime().toString() + "." + fileExtension;
    };

    var postfile = function () {
        if (_fileUploaded) {
            $("#" + _submitButtonName).button("loading");
            $("#" + _submitFormName).submit();
            return;
        }
        var fileExtention =  $("#" + _emailFormField.fileType + ":checked")!=undefined?$("#" + _emailFormField.fileType + ":checked").val():"xlsx";
        var grid = $("#" + _gridName).data("kendoGrid");
        var file = generateFileName(fileExtention);

        grid.setOptions({
            excelExport: function (e) {
                // Prevent the default behavior which will prompt the user to save the generated file.
                e.preventDefault();
                // Get the Excel file as a data URL.
                var dataURL = new kendo.ooxml.Workbook(e.workbook).toDataURL();
                // Strip the data URL prologue.
                var base64 = dataURL.split(";base64,")[1];
                // Post the base64 encoded content to the server which can save it.
                var post = $.post(_saveUrl, {
                    base64: base64,
                    fileName: file
                });
                $("#" + _submitButtonName).button("loading");
                post.done(function (response) {
                    if (response.Success) {
                        _fileUploaded = true;
                        $("#" + _emailFormField.fileName).val(file);
                        $("#" + _gridRefreshTriggerName).trigger("click");
                        $("#" + _submitFormName).submit();
                    }
                    else {
                        $("#" + _submitButtonName).button("reset");
                        Metronic.alert({
                            container: "#"+_responseMessage, // alerts parent container(by default placed after the page breadcrumbs)
                            place: "prepend", // append or prepent in container 
                            type: "danger",  // alert's type
                            message: response.Message,  // alert's message
                            close: true, // make alert closable
                            reset: true, // close all previouse alerts first
                            focus: true, // auto scroll to the alert after shown
                            closeInSeconds: 0, // auto close after defined seconds
                            icon: "warning" // put icon before the message
                        });
                      
                    }
                    
                });
            }
        });
        grid.saveAsExcel();
        
    }

    this.getFormData = function () {
        return {
            Email: $("#" + _emailFormField.emailAddress).val(),
            FileName: $("#" + _emailFormField.fileName).val(),
            Subject: $("#" + _emailFormField.subject).val(),
            Message: $("#" + _emailFormField.message).val()
        }
    }
    this.submitForm = function (form) {
        $.ajax({
            type: form.method,
            url: form.action,
            data: myThis.getFormData(),
            dataType: 'json'
        }).done(function (response) {
            if (response.Success) {
                Metronic.alert({
                    container: "#" + _responseMessage, // alerts parent container(by default placed after the page breadcrumbs)
                    place: "prepend", // append or prepent in container 
                    type: "success",  // alert's type
                    message: response.Message,  // alert's message
                    close: true, // make alert closable
                    reset: true, // close all previouse alerts first
                    focus: true, // auto scroll to the alert after shown
                    closeInSeconds: 10, // auto close after defined seconds
                    icon: "check" // put icon before the message
                });
                $("#" + _submitButtonName).button("reset");
                myThis.resetForm(form);
            }
            else {
                Metronic.alert({
                    container: "#" + _responseMessage, // alerts parent container(by default placed after the page breadcrumbs)
                    place: "prepend", // append or prepent in container 
                    type: "danger",  // alert's type
                    message: response.Message,  // alert's message
                    close: true, // make alert closable
                    reset: true, // close all previouse alerts first
                    focus: true, // auto scroll to the alert after shown
                    closeInSeconds: 0, // auto close after defined seconds
                    icon: "warning" // put icon before the message
                });
                $("#" + _submitButtonName).button("reset");
            }
        });
    }

    ManualEmailSender.prototype.registerInstance = function () {
        myThis = this;
    }
    var validator = null;

    var formReset = function (form) {
        $(form).find("input[type=text], textarea, input[type=hidden]").each(function (index, element) {
            $(element).val("");
        });
        $('#' + _emailFormField.message).code('');
        _fileUploaded = false;
        validator.resetForm();
    }

    this.resetForm = function (form) {
        formReset(form);
    }
    this.init = function () {
        this.registerInstance();
        $("#" + _emailFormField.message).summernote({
            height: 200,
            toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            ['fontname', ['fontname']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['hr']],
            ['view', ['fullscreen', 'codeview']]]

        });

        $("input[name='"+_emailFormField.fileType+"']").iCheck({
            radioClass: "iradio_square-grey"
        });

        $("#" + _submitButtonName).on("click", function (e) {
            postfile();
        });

        $("#" + _submitFormName).attr("action", _messageSaveUrl);
        var $modal = $("#" + _submitFormName).closest(".modal");

        $($modal).on('hidden.bs.modal', function () {
            formReset($("#" + _submitFormName));
        });

        validator = $("#" + _submitFormName).validate({
            // ajax submit
            errorElement: 'span', //default input error message container
            errorClass: 'help-block help-block-error', // default input error message class
            focusInvalid: false, // do not focus the last invalid input
            ignore: "input[class|='note'], textarea[class|='note'] ", // validate all fields including form hidden input
            submitHandler: function (form) {
                myThis.submitForm(form);
                return false;
            },
            invalidHandler: function (event, validator) { //display error alert on form submit              
                $("#" + _submitButtonName).button("reset");
            },

            errorPlacement: function (error, element) { // render error placement for each input type
                if (element.parent(".input-group").size() > 0) {
                    error.insertAfter(element.parent(".input-group"));
                } else if (element.attr("data-error-container")) {
                    error.appendTo(element.attr("data-error-container"));
                } else if (element.parents('.radio-list').size() > 0) {
                    error.appendTo(element.parents('.radio-list').attr("data-error-container"));
                } else if (element.parents('.radio-inline').size() > 0) {
                    error.appendTo(element.parents('.radio-inline').attr("data-error-container"));
                } else if (element.parents('.checkbox-list').size() > 0) {
                    error.appendTo(element.parents('.checkbox-list').attr("data-error-container"));
                } else if (element.parents('.checkbox-inline').size() > 0) {
                    error.appendTo(element.parents('.checkbox-inline').attr("data-error-container"));
                } else {
                    error.insertAfter(element); // for other inputs, just perform default behavior
                }
            },

            highlight: function (element) { // hightlight error inputs
                $(element).closest('.form-group').addClass('has-error'); // set error class to the control group  
            },

            unhighlight: function (element) { // revert the change done by hightlight
                $(element).closest('.form-group').removeClass('has-error'); // set error class to the control group
            },

            success: function (label, element) {
                label.closest('.form-group').removeClass('has-error'); // set success class to the control group
            },
        });

        $("#" + _emailFormField.emailAddress).rules("add", {
            required: true,
        });
        $("#" + _emailFormField.emailAddress).rules("add", {
            multipleEmail: true,
        });

        $("#" + _emailFormField.fileName).rules("add", {
            required: true,
        });

        $("#" + _emailFormField.subject).rules("add", {
            required: true
        });
        $("#" + _emailFormField.message).rules("add", {
            required: true
        });
    }
}