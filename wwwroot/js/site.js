(function () {
    // 1. Light / Dark Mode Persistence
    const html = document.documentElement;
    const savedTheme = localStorage.getItem('nyayasetu_theme') || 'light';
    html.setAttribute('data-theme', savedTheme);

    const themeBtn = document.getElementById('themeToggle');
    if (themeBtn) {
        themeBtn.addEventListener('click', () => {
            const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            html.setAttribute('data-theme', next);
            localStorage.setItem('nyayasetu_theme', next);
        });
    }

    // 2. High Contrast Accessibility Toggle
    const contrastBtn = document.getElementById('contrastToggle');
    if (localStorage.getItem('nyayasetu_contrast') === 'on') {
        document.body.classList.add('high-contrast');
    }
    if (contrastBtn) {
        contrastBtn.addEventListener('click', () => {
            document.body.classList.toggle('high-contrast');
            localStorage.setItem('nyayasetu_contrast', document.body.classList.contains('high-contrast') ? 'on' : 'off');
        });
    }

    // 3. Optional Voice-Assisted Search (Web Speech API)
    const voiceBtn = document.getElementById('voiceSearchBtn');
    const searchInput = document.getElementById('heroSearchInput');
    if (voiceBtn && searchInput) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            voiceBtn.title = 'Voice search is not supported in this browser';
            voiceBtn.disabled = true;
        } else {
            voiceBtn.addEventListener('click', () => {
                const recognition = new SpeechRecognition();
                recognition.lang = 'en-IN';
                recognition.interimResults = false;
                voiceBtn.classList.add('btn-warning');
                recognition.onresult = (event) => {
                    const transcript = event.results[0][0].transcript;
                    searchInput.value = transcript;
                };
                recognition.onend = () => voiceBtn.classList.remove('btn-warning');
                recognition.start();
            });
        }
    }

    // 4. Unread Notification Badge Polling (Dashboard Layouts)
    const notifBadge = document.getElementById('notifBadge');
    if (notifBadge) {
        fetch('/Notifications/UnreadCount')
            .then(r => r.ok ? r.json() : null)
            .then(data => {
                if (data && data.count > 0) {
                    notifBadge.textContent = data.count;
                    notifBadge.classList.remove('d-none');
                }
            })
            .catch(() => {});
    }

    // 5. AI Legal Information Assistant AJAX Handler
    const assistantForm = document.getElementById('assistantForm');
    const assistantInput = document.getElementById('assistantInput');
    const chatMessages = document.getElementById('chatMessages');
    if (assistantForm && assistantInput && chatMessages) {
        assistantForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const q = assistantInput.value.trim();
            if (!q) return;

            appendBubble('You', q, 'bg-primary text-white ms-auto');
            assistantInput.value = '';

            try {
                const res = await fetch('/Assistant/Ask', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ question: q })
                });
                const data = await res.json();
                let linksHtml = '';
                if (data.sources && data.sources.length) {
                    linksHtml = '<div class="mt-2 pt-2 border-top small"><strong>Official Sources:</strong> ' +
                        data.sources.map(s => `<a href="${s.url}" target="_blank" rel="noopener" class="me-2">${s.title}</a>`).join('') +
                        '</div>';
                }
                const disclaimerHtml = `<div class="mt-2 text-muted" style="font-size:0.75rem;">${data.disclaimer}</div>`;
                appendBubble('NyayaSetu Assistant', `${data.answer}${linksHtml}${disclaimerHtml}`, 'bg-white');
            } catch {
                appendBubble('NyayaSetu Assistant', 'Unable to reach the assistant service right now.', 'bg-white');
            }
        });
    }

    function appendBubble(sender, htmlContent, classes) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mb-3';
        wrapper.innerHTML = `<div class="small fw-semibold mb-1">${sender}</div><div class="p-3 rounded-3 shadow-sm small ${classes}">${htmlContent}</div>`;
        chatMessages.appendChild(wrapper);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
})();
