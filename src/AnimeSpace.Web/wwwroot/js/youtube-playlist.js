(() => {
    const iframe = document.querySelector('iframe[data-youtube-playlist]');
    const panel = document.getElementById('episode-panel');
    const list = document.getElementById('episode-list');
    if (!iframe || !panel || !list) return;

    let player = null;
    let built = false;

    function buildButtons(videoIds) {
        list.replaceChildren();

        videoIds.forEach((_, index) => {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'btn btn-sm btn-outline-secondary episode-btn';
            button.textContent = String(index + 1);
            button.title = `Серія ${index + 1}`;
            button.dataset.index = String(index);
            list.appendChild(button);
        });

        panel.classList.remove('d-none');
        built = true;
        highlight();
    }

    function highlight() {
        if (!built || !player || typeof player.getPlaylistIndex !== 'function') return;

        const current = player.getPlaylistIndex();
        list.querySelectorAll('.episode-btn').forEach(button => {
            const active = Number(button.dataset.index) === current;
            button.classList.toggle('btn-primary', active);
            button.classList.toggle('btn-outline-secondary', !active);
        });
    }

    function tryBuild() {
        if (built || !player || typeof player.getPlaylist !== 'function') return built;

        const videoIds = player.getPlaylist();
        if (Array.isArray(videoIds) && videoIds.length > 0) {
            buildButtons(videoIds);
        }

        return built;
    }

    list.addEventListener('click', event => {
        const button = event.target.closest('.episode-btn');
        if (!button || !player) return;

        player.playVideoAt(Number(button.dataset.index));
    });

    function init() {
        player = new window.YT.Player(iframe, {
            events: {
                onReady: () => {
                    tryBuild();
                },
                onStateChange: () => {
                    tryBuild();
                    highlight();
                }
            }
        });

        let attempts = 0;
        const timer = setInterval(() => {
            attempts += 1;
            if (tryBuild() || attempts >= 30) clearInterval(timer);
        }, 500);
    }

    if (window.YT && window.YT.Player) {
        init();
    } else {
        const previous = window.onYouTubeIframeAPIReady;

        window.onYouTubeIframeAPIReady = () => {
            if (typeof previous === 'function') previous();
            init();
        };

        const script = document.createElement('script');
        script.src = 'https://www.youtube.com/iframe_api';
        document.head.appendChild(script);
    }
})();
