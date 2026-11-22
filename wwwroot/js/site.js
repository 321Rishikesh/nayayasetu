/**
 * NyayaSetu – Smart Law & Justice Management System
 * Core Client-side Script: Dual Theme Engine (Light/Dark), Accessibility & Interactive Features
 */

(function () {
    'use strict';

    const html = document.documentElement;

    // =========================================================================
    // 1. Theme Management (Dual Attribute Sync: data-theme & data-bs-theme)
    // =========================================================================
    function applyTheme(theme) {
        html.setAttribute('data-theme', theme);
        html.setAttribute('data-bs-theme', theme);
        localStorage.setItem('nyayasetu_theme', theme);

        // Update all toggle buttons in DOM
        const toggleButtons = document.querySelectorAll('.theme-toggle-btn, #themeToggle, [data-theme-toggle]');
        toggleButtons.forEach(btn => {
            const icon = btn.querySelector('i');
            const label = btn.querySelector('.theme-toggle-label');
            if (theme === 'dark') {
                if (icon) {
                    icon.className = 'fa-solid fa-sun text-warning me-1';
                }
                if (label) {
                    label.textContent = 'Light';
                }
                btn.setAttribute('title', 'Switch to Light Mode');
                btn.setAttribute('aria-label', 'Switch to Light Mode');
            } else {
                if (icon) {
                    icon.className = 'fa-solid fa-moon me-1';
                }
                if (label) {
                    label.textContent = 'Dark';
                }
                btn.setAttribute('title', 'Switch to Dark Mode');
                btn.setAttribute('aria-label', 'Switch to Dark Mode');
            }
        });

        // Adapt Chart.js defaults if library is present
        if (window.Chart) {
            if (theme === 'dark') {
                Chart.defaults.color = '#CBD5E1';
                Chart.defaults.borderColor = '#223249';
            } else {
                Chart.defaults.color = '#475569';
                Chart.defaults.borderColor = '#E2E8F0';
            }
            // Trigger chart redraws if instances exist
            if (window.Chart.instances) {
                Object.values(window.Chart.instances).forEach(chart => chart.update());
            }
        }
    }

    // Initial theme setup on page load
    const savedTheme = localStorage.getItem('nyayasetu_theme') || 
        (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    applyTheme(savedTheme);

    // Event Delegation for Theme Toggle Buttons
    document.addEventListener('click', (e) => {
        const toggleBtn = e.target.closest('.theme-toggle-btn, #themeToggle, [data-theme-toggle]');
        if (toggleBtn) {
            e.preventDefault();
            const currentTheme = html.getAttribute('data-theme') || 'light';
            const nextTheme = currentTheme === 'dark' ? 'light' : 'dark';
            applyTheme(nextTheme);
        }
    });

    // =========================================================================
    // 2. Accessibility Controls: Font Resizer & High Contrast
    // =========================================================================
    function setFontSize(size) {
        document.body.classList.remove('font-sm', 'font-md', 'font-lg');
        if (size === 'sm') document.body.classList.add('font-sm');
        else if (size === 'lg') document.body.classList.add('font-lg');
        else document.body.classList.add('font-md');
        localStorage.setItem('nyayasetu_font_size', size);
    }

    const savedFontSize = localStorage.getItem('nyayasetu_font_size') || 'md';
    setFontSize(savedFontSize);

    document.addEventListener('click', (e) => {
        const fontBtn = e.target.closest('[data-font-size]');
        if (fontBtn) {
            e.preventDefault();
            const size = fontBtn.getAttribute('data-font-size');
            setFontSize(size);
        }
    });

    // High Contrast Accessibility Toggle
    if (localStorage.getItem('nyayasetu_contrast') === 'on') {
        document.body.classList.add('high-contrast');
    }

    document.addEventListener('click', (e) => {
        const contrastBtn = e.target.closest('#contrastToggle, [data-contrast-toggle]');
        if (contrastBtn) {
            e.preventDefault();
            document.body.classList.toggle('high-contrast');
            localStorage.setItem('nyayasetu_contrast', document.body.classList.contains('high-contrast') ? 'on' : 'off');
        }
    });

    // =========================================================================
    // 3. Voice-Assisted Search (Web Speech API)
    // =========================================================================
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
                voiceBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin text-dark"></i>';

                recognition.onresult = (event) => {
                    const transcript = event.results[0][0].transcript;
                    searchInput.value = transcript.replace(/\.$/, '');
                };
                recognition.onerror = () => {
                    voiceBtn.classList.remove('btn-warning');
                    voiceBtn.innerHTML = '<i class="fa-solid fa-microphone text-primary"></i>';
                };
                recognition.onend = () => {
                    voiceBtn.classList.remove('btn-warning');
                    voiceBtn.innerHTML = '<i class="fa-solid fa-microphone text-primary"></i>';
                };
                recognition.start();
            });
        }
    }

    // =========================================================================
    // 4. Notification Badge Poller (Dashboard Workspaces)
    // =========================================================================
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

    // =========================================================================
    // 5. AI Legal Information Assistant (Interactive Form & Dark Mode bubbles)
    // =========================================================================
    const assistantForm = document.getElementById('assistantForm');
    const assistantInput = document.getElementById('assistantInput');
    const chatMessages = document.getElementById('chatMessages');

    if (assistantForm && assistantInput && chatMessages) {
        assistantForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const q = assistantInput.value.trim();
            if (!q) return;

            appendBubble('You', q, 'bg-primary text-white ms-auto border-0');
            assistantInput.value = '';

            // Show typing indicator
            const typingWrapper = document.createElement('div');
            typingWrapper.id = 'assistantTyping';
            typingWrapper.className = 'mb-3';
            typingWrapper.innerHTML = `
                <div class="small fw-semibold text-primary mb-1"><i class="fa-solid fa-robot me-1"></i>NyayaSetu Sahayak</div>
                <div class="p-3 rounded-3 shadow-sm small chat-bubble-assistant d-inline-block">
                    <i class="fa-solid fa-circle-notch fa-spin me-2"></i>Analyzing legal sources...
                </div>
            `;
            chatMessages.appendChild(typingWrapper);
            chatMessages.scrollTop = chatMessages.scrollHeight;

            try {
                const res = await fetch('/Assistant/Ask', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ question: q })
                });
                const data = await res.json();
                
                const typing = document.getElementById('assistantTyping');
                if (typing) typing.remove();

                let linksHtml = '';
                if (data.sources && data.sources.length) {
                    linksHtml = '<div class="mt-2 pt-2 border-top small text-muted"><strong>Official Portals:</strong> ' +
                        data.sources.map(s => `<a href="${s.url}" target="_blank" rel="noopener" class="me-2 text-primary fw-semibold">${s.title}</a>`).join('') +
                        '</div>';
                }
                const disclaimerHtml = `<div class="mt-2 text-muted" style="font-size:0.75rem;"><i class="fa-solid fa-scale-balanced me-1"></i>${data.disclaimer}</div>`;
                appendBubble('NyayaSetu Sahayak', `${data.answer}${linksHtml}${disclaimerHtml}`, 'chat-bubble-assistant');
            } catch {
                const typing = document.getElementById('assistantTyping');
                if (typing) typing.remove();
                appendBubble('NyayaSetu Sahayak', 'Unable to reach the assistant service right now. Please try again.', 'chat-bubble-assistant text-danger');
            }
        });
    }

    function appendBubble(sender, htmlContent, classes) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mb-3';
        const isUser = sender === 'You';
        wrapper.innerHTML = `
            <div class="small fw-semibold mb-1 ${isUser ? 'text-end text-muted' : 'text-primary'}">${isUser ? '<i class="fa-solid fa-user me-1"></i>You' : '<i class="fa-solid fa-robot me-1"></i>' + sender}</div>
            <div class="p-3 rounded-3 shadow-sm small ${classes}" style="max-width: 85%; ${isUser ? 'margin-left: auto;' : ''}">${htmlContent}</div>
        `;
        chatMessages.appendChild(wrapper);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
})();