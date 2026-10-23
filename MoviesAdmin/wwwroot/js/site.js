// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function() {
    const form = document.querySelector(
        "form[data-confirm-missing-poster]"
    );

    const modalElement = document.getElementById(
        "missingPosterModal"
    );

    if (!form || !modalElement) {
        return;
    }

    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);

    const confirmButton = document.getElementById(
        "confirmWithoutPoster"
    );

    let approved = false;

    form.addEventListener("submit", function(event) {
        if (approved) {
            return;
        }

        // Validate the existing movie fields first.
        const browserValid = form.checkValidity();

        const jqueryValid = window.jQuery &&
            typeof window.jQuery.fn.valid === "function"
            ? window.jQuery(form).valid()
            : true;

        if (!browserValid || !jqueryValid) {
            event.preventDefault();

            if (!browserValid) {
                form.reportValidity();
            }

            return;
        }

        // Only check the poster after other validation succeeds.
        const posterInput = form.querySelector(
            'input[type="file"][name="posterFile"]'
        );

        const hasNewPoster = (posterInput?.files.length ?? 0) > 0;
        const hasExistingPoster = form.dataset.hasPoster === "true";

        if (!hasNewPoster && !hasExistingPoster) {
            event.preventDefault();
            modal.show();
        }
    });

    confirmButton.addEventListener("click", function() {
        approved = true;

        modal.hide();
        form.requestSubmit();

        approved = false;
    });
});
