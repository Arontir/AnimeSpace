(() => {
    const ratingSelect = document.getElementById('rating-select');
    const ratingSummary = document.getElementById('rating-summary');
    const ratingFeedback = document.getElementById('rating-feedback');

    const commentForm = document.getElementById('comment-form');
    const commentList = document.getElementById('comment-list');
    const commentCount = document.getElementById('comment-count');
    const commentFeedback = document.getElementById('comment-feedback');
    const noComments = document.getElementById('no-comments');
    const template = document.getElementById('comment-template');

    function showFeedback(element, message, isError) {
        if (!element) return;
        element.textContent = message;
        element.classList.toggle('text-danger', isError);
        element.classList.toggle('text-success', !isError);
    }

    if (ratingSelect) {
        ratingSelect.addEventListener('change', async () => {
            const score = Number(ratingSelect.value);
            if (!score) return;

            ratingSelect.disabled = true;
            try {
                const result = await window.ajax.post('/Rating/Set', {
                    animeId: Number(ratingSelect.dataset.animeId),
                    score
                });

                ratingSummary.textContent = `★ ${result.averageRating.toFixed(1)} (${result.ratingsCount})`;
                ratingSummary.classList.remove('text-secondary');
                ratingSummary.classList.add('text-warning');
                showFeedback(ratingFeedback, 'Дякуємо за оцінку!', false);
            } catch (error) {
                showFeedback(ratingFeedback, error.message, true);
            } finally {
                ratingSelect.disabled = false;
            }
        });
    }

    function formatUtc(value) {
        const iso = /(?:[zZ]|[+-]\d\d:\d\d)$/.test(value) ? value : `${value}Z`;
        const date = new Date(iso);
        const pad = n => String(n).padStart(2, '0');

        return `${pad(date.getUTCDate())}.${pad(date.getUTCMonth() + 1)}.${date.getUTCFullYear()} `
            + `${pad(date.getUTCHours())}:${pad(date.getUTCMinutes())} UTC`;
    }

    function createCommentElement(comment) {
        const fragment = template.content.cloneNode(true);

        fragment.querySelector('.comment-item').dataset.commentId = comment.id;
        fragment.querySelector('.comment-author').textContent = comment.userName;
        fragment.querySelector('.comment-date').textContent = formatUtc(comment.createdAt);
        fragment.querySelector('.comment-text').textContent = comment.text;
        fragment.querySelector('.comment-delete').dataset.commentId = comment.id;

        return fragment;
    }

    function changeCount(delta) {
        const current = (Number(commentCount.textContent) || 0) + delta;
        commentCount.textContent = current;
        noComments?.classList.toggle('d-none', current > 0);
    }

    if (commentForm) {
        commentForm.addEventListener('submit', async event => {
            event.preventDefault();

            const textarea = commentForm.querySelector('textarea[name="text"]');
            const button = commentForm.querySelector('button[type="submit"]');
            const text = textarea.value.trim();
            if (!text) return;

            button.disabled = true;
            try {
                const comment = await window.ajax.post('/Comment/Add', {
                    animeId: Number(commentForm.dataset.animeId),
                    text
                });

                commentList.prepend(createCommentElement(comment));
                changeCount(1);
                textarea.value = '';
                showFeedback(commentFeedback, '', false);
            } catch (error) {
                showFeedback(commentFeedback, error.message, true);
            } finally {
                button.disabled = false;
            }
        });
    }

    if (commentList) {
        commentList.addEventListener('click', async event => {
            const button = event.target.closest('.comment-delete');
            if (!button) return;
            if (!window.confirm('Видалити коментар?')) return;

            button.disabled = true;
            try {
                await window.ajax.post(`/Comment/Delete?id=${encodeURIComponent(button.dataset.commentId)}`);
                button.closest('.comment-item').remove();
                changeCount(-1);
                showFeedback(commentFeedback, '', false);
            } catch (error) {
                button.disabled = false;
                showFeedback(commentFeedback, error.message, true);
            }
        });
    }
})();
