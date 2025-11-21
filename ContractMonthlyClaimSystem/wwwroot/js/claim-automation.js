// FILE LOCATION: /wwwroot/js/claim-automation.js
// AUTOMATED CLAIM SUBMISSION WITH REAL-TIME VALIDATION

$(document).ready(function () {
    // Auto-calculate total amount when hours or rate change
    $('#TotalHours, #HourlyRate').on('input', function () {
        calculateTotal();
    });

    // Initialize on page load
    calculateTotal();

    // File upload validation
    $('#SupportingDocuments').on('change', function () {
        validateFileUploads();
    });
});

// AUTOMATED CALCULATION FUNCTION
function calculateTotal() {
    const hours = parseFloat($('#TotalHours').val()) || 0;
    const rate = parseFloat($('#HourlyRate').val()) || 0;
    const total = hours * rate;

    // Update display
    $('#TotalAmount').val('R ' + total.toFixed(2));
    $('#TotalAmountDisplay').text('R ' + total.toLocaleString('en-ZA', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }));

    // Real-time validation
    validateClaim(hours, rate, total);
}

// AUTOMATED VALIDATION FUNCTION
function validateClaim(hours, rate, total) {
    let isValid = true;
    let errors = [];

    // Validate hours
    if (hours < 1 || hours > 744) {
        errors.push('⚠️ Hours must be between 1 and 744');
        isValid = false;
        $('#TotalHours').addClass('is-invalid');
    } else {
        $('#TotalHours').removeClass('is-invalid').addClass('is-valid');
    }

    // Validate rate
    if (rate < 50 || rate > 500) {
        errors.push('⚠️ Hourly rate must be between R50 and R500');
        isValid = false;
        $('#HourlyRate').addClass('is-invalid');
    } else {
        $('#HourlyRate').removeClass('is-invalid').addClass('is-valid');
    }

    // Validate total
    if (total > 100000) {
        errors.push('⚠️ Total amount seems unusually high. Please verify.');
        isValid = false;
    }

    // Display validation results
    if (errors.length > 0) {
        $('#validationErrors').html('<div class="alert alert-warning">' + errors.join('<br>') + '</div>').show();
        $('#submitButton').prop('disabled', true);
    } else {
        $('#validationErrors').hide();
        $('#submitButton').prop('disabled', false);
    }

    return isValid;
}

// AUTOMATED FILE VALIDATION
function validateFileUploads() {
    const files = document.getElementById('SupportingDocuments').files;
    const maxSize = 5 * 1024 * 1024; // 5MB
    const allowedExtensions = ['.pdf', '.docx', '.xlsx'];
    let isValid = true;
    let errors = [];

    if (files.length === 0) {
        $('#fileValidation').html('<div class="alert alert-info">No files selected</div>');
        return true;
    }

    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const fileName = file.name.toLowerCase();
        const fileExtension = fileName.substring(fileName.lastIndexOf('.'));

        // Check file size
        if (file.size > maxSize) {
            errors.push(`❌ ${file.name} is too large (max 5MB)`);
            isValid = false;
        }

        // Check file type
        if (!allowedExtensions.includes(fileExtension)) {
            errors.push(`❌ ${file.name} has invalid type (only PDF, DOCX, XLSX allowed)`);
            isValid = false;
        }
    }

    // Display results
    if (errors.length > 0) {
        $('#fileValidation').html('<div class="alert alert-danger">' + errors.join('<br>') + '</div>').show();
    } else {
        $('#fileValidation').html('<div class="alert alert-success">✅ ' + files.length + ' file(s) ready to upload</div>').show();
    }

    return isValid;
}

// ANIMATED PROGRESS BAR FOR SUBMISSION
function showSubmissionProgress() {
    $('#submitButton').prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Submitting...');

    // Simulate progress
    let progress = 0;
    const progressBar = $('<div class="progress mt-3"><div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar"></div></div>');
    $('#submitButton').after(progressBar);

    const interval = setInterval(function () {
        progress += 10;
        progressBar.find('.progress-bar').css('width', progress + '%').attr('aria-valuenow', progress);

        if (progress >= 100) {
            clearInterval(interval);
        }
    }, 200);
}

// AUTOMATED MONTH VALIDATION
$('#ClaimMonth').on('change', function () {
    const selectedMonth = new DateTime($(this).val());
    const currentMonth = new Date();

    if (selectedMonth > currentMonth) {
        alert('⚠️ Cannot submit claims for future months');
        $(this).val('');
    }
});

// TOOLTIPS FOR AUTOMATION FEATURES
$('[data-toggle="tooltip"]').tooltip();

// KEYBOARD SHORTCUTS FOR EFFICIENCY
$(document).keydown(function (e) {
    // Ctrl+Enter to submit
    if (e.ctrlKey && e.which === 13) {
        if (!$('#submitButton').prop('disabled')) {
            $('#submitButton').click();
        }
    }
});