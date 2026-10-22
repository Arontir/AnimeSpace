window.ajax = (() => {
    const csrfToken = () => document.querySelector('meta[name="csrf-token"]')?.content ?? '';

    function parse(text) {
        if (!text) return null;
        try {
            return JSON.parse(text);
        } catch {
            return null;
        }
    }

    function messageFrom(data) {
        if (!data) return null;
        if (typeof data.error === 'string') return data.error;

        if (data.errors && typeof data.errors === 'object') {
            const first = Object.values(data.errors).flat()[0];
            if (typeof first === 'string') return first;
        }

        return typeof data.title === 'string' ? data.title : null;
    }

    async function post(url, body) {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken()
            },
            body: body === undefined ? undefined : JSON.stringify(body)
        });

        if (response.status === 401) {
            const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = `/Identity/Account/Login?ReturnUrl=${returnUrl}`;
            throw new Error('Потрібно увійти.');
        }

        const data = parse(await response.text());

        if (!response.ok) {
            const fallback = response.status === 403
                ? 'Недостатньо прав для цієї дії.'
                : 'Не вдалося виконати дію. Спробуйте ще раз.';
            throw new Error(messageFrom(data) ?? fallback);
        }

        return data;
    }

    return { post };
})();
