(() => {
    function showFeedback(element, message, isError) {
        if (!element) return;
        element.textContent = message;
        element.classList.toggle('text-danger', isError);
        element.classList.toggle('text-success', !isError);
    }

    document.querySelectorAll('[data-bookmark-select]').forEach(select => {
        select.addEventListener('change', async () => {
            const status = Number(select.value);
            if (!status) return;

            const box = select.closest('[data-bookmark-box]');
            const feedback = box?.querySelector('[data-bookmark-feedback]');
            const removeButton = box?.querySelector('[data-bookmark-remove]');

            select.disabled = true;
            try {
                await window.ajax.post('/Bookmark/ChangeStatus', {
                    animeId: Number(select.dataset.animeId),
                    status
                });

                if (select.dataset.reload === 'true') {
                    window.location.reload();
                    return;
                }

                removeButton?.classList.remove('d-none');
                showFeedback(feedback, 'Збережено', false);
            } catch (error) {
                showFeedback(feedback, error.message, true);
            } finally {
                select.disabled = false;
            }
        });
    });

    document.querySelectorAll('[data-bookmark-remove]').forEach(button => {
        button.addEventListener('click', async () => {
            const box = button.closest('[data-bookmark-box]');
            const feedback = box?.querySelector('[data-bookmark-feedback]');

            button.disabled = true;
            try {
                await window.ajax.post(`/Bookmark/Delete?animeId=${encodeURIComponent(button.dataset.animeId)}`);

                if (button.dataset.reload === 'true') {
                    window.location.reload();
                    return;
                }

                const select = box?.querySelector('[data-bookmark-select]');
                if (select) select.value = '';
                button.classList.add('d-none');
                showFeedback(feedback, 'Прибрано зі списку', false);
            } catch (error) {
                showFeedback(feedback, error.message, true);
            } finally {
                button.disabled = false;
            }
        });
    });

    const tabList = document.getElementById('profileTabs');
    if (tabList) {
        const storageKey = 'profileTab';
        const saved = sessionStorage.getItem(storageKey);

        if (saved) {
            const trigger = tabList.querySelector(`[data-bs-target="${saved}"]`);
            if (trigger) window.bootstrap.Tab.getOrCreateInstance(trigger).show();
        }

        tabList.addEventListener('shown.bs.tab', event => {
            sessionStorage.setItem(storageKey, event.target.getAttribute('data-bs-target'));
        });
    }
})();
