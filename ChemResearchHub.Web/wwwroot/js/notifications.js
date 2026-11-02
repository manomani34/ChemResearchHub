(() => {
    "use strict";

    const container = document.getElementById("notificationContainer");
    if (!container) return;

    const trigger = document.getElementById("notificationTrigger");
    const dropdown = document.getElementById("notificationDropdown");
    const countBadge = document.getElementById("notificationCount");
    const headerCount = document.getElementById("notificationHeaderCount");
    const list = document.getElementById("notificationList");
    const markAllButton = document.getElementById("markAllNotificationsRead");
    const tokenInput = document.querySelector(
        '#notificationAntiforgeryForm input[name="__RequestVerificationToken"]'
    );

    const token = tokenInput?.value ?? "";

    const endpoints = {
        list: "/User/Notifications?page=1",
        unreadCount: "/User/Notifications/unread-count",
        markAll: "/User/Notifications/read-all",
        markRead: id => `/User/Notifications/${encodeURIComponent(id)}/read`
    };

    let notifications = [];

        // Notification sound
    let audioContext = null;
    let soundEnabled = false;

    function enableNotificationSound() {
        if (soundEnabled) return;

        const AudioContextClass =
            window.AudioContext || window.webkitAudioContext;

        if (!AudioContextClass) return;

        audioContext ??= new AudioContextClass();

        if (audioContext.state === "suspended") {
            audioContext.resume();
        }

        soundEnabled = true;
    }

    function playNotificationSound() {
        if (!soundEnabled || !audioContext) return;

        const now = audioContext.currentTime;
        const oscillator = audioContext.createOscillator();
        const gain = audioContext.createGain();

        oscillator.type = "sine";
        oscillator.frequency.setValueAtTime(880, now);
        oscillator.frequency.setValueAtTime(1174.66, now + 0.12);

        gain.gain.setValueAtTime(0.0001, now);
        gain.gain.exponentialRampToValueAtTime(0.12, now + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.35);

        oscillator.connect(gain);
        gain.connect(audioContext.destination);

        oscillator.start(now);
        oscillator.stop(now + 0.36);
    }

    // Enable sound after the first user interaction.
    document.addEventListener("click", enableNotificationSound, {
        once: true
    });

    function getValue(item, pascal, camel) {
        return item?.[camel] ?? item?.[pascal];
    }

    async function request(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            ...options,
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                ...(options.headers || {})
            }
        });

        if (!response.ok) {
            throw new Error(`Request failed: ${response.status}`);
        }

        return response.status === 204 ? null : response.json();
    }

    async function loadUnreadCount() {
        try {
            const data = await request(endpoints.unreadCount);
            const count = Number(data?.count ?? data?.Count ?? 0);

            countBadge.textContent = count > 99 ? "99+" : String(count);
            countBadge.hidden = count <= 0;

            headerCount.textContent = count > 0
                ? `${count} اعلان خوانده‌نشده`
                : "همه اعلان‌ها خوانده شده‌اند";

            markAllButton.disabled = count <= 0;
        } catch (error) {
            console.error("Notification count error:", error);
        }
    }

        // Detect newly received notifications.
    let lastNotificationId = null;
    let notificationBaselineInitialized = false;

    async function checkForNewNotifications() {
        try {
            const data = await request(endpoints.list);
            const items = data?.notifications ?? data?.Notifications ?? [];

            if (!items.length) return;

            const newestId = getValue(items[0], "Id", "id");

            if (!notificationBaselineInitialized) {
                lastNotificationId = newestId;
                notificationBaselineInitialized = true;
                return;
            }

            if (
                newestId != null &&
                lastNotificationId != null &&
                String(newestId) !== String(lastNotificationId)
            ) {
                lastNotificationId = newestId;
                playNotificationSound();

                await loadUnreadCount();

                if (!dropdown.hidden) {
                    await loadNotifications();
                }
            }
        } catch (error) {
            console.error("New notification check error:", error);
        }
    }

    function formatDate(value) {
        if (!value) return "";

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return "";

        return new Intl.DateTimeFormat("fa-IR", {
            dateStyle: "short",
            timeStyle: "short"
        }).format(date);
    }

    function createNotificationElement(item) {
        const id = getValue(item, "Id", "id");
        const title = getValue(item, "Title", "title") ?? "";
        const message = getValue(item, "Message", "message") ?? "";
        const actionUrl = getValue(item, "ActionUrl", "actionUrl");
        const createdAt = getValue(item, "CreatedAt", "createdAt");
        const isRead = Boolean(getValue(item, "IsRead", "isRead"));

        const element = document.createElement("article");
        element.className = `notification-item${isRead ? "" : " unread"}`;
        element.dataset.id = id ?? "";

        const content = document.createElement("div");
        content.className = "notification-item-content";

        const titleElement = document.createElement("div");
        titleElement.className = "notification-item-title";
        titleElement.textContent = title;

        const messageElement = document.createElement("div");
        messageElement.className = "notification-item-message";
        messageElement.textContent = message;

        const timeElement = document.createElement("div");
        timeElement.className = "notification-item-time";
        timeElement.textContent = formatDate(createdAt);

        content.append(titleElement, messageElement, timeElement);
        element.appendChild(content);

        if (!isRead) {
            const dot = document.createElement("span");
            dot.className = "notification-unread-dot";
            dot.setAttribute("aria-label", "خوانده‌نشده");
            element.appendChild(dot);
        }

        element.addEventListener("click", () => {
            handleNotificationClick(id, actionUrl, isRead);
        });

        return element;
    }

    function renderNotifications(items) {
        list.replaceChildren();

        if (!items.length) {
            const empty = document.createElement("div");
            empty.className = "notification-empty";
            empty.textContent = "اعلانی برای نمایش وجود ندارد.";
            list.appendChild(empty);
            return;
        }

        items.forEach(item => {
            list.appendChild(createNotificationElement(item));
        });
    }

    async function loadNotifications() {
        list.textContent = "در حال دریافت اعلان‌ها...";

        try {
            const data = await request(endpoints.list);
            notifications = data?.notifications ?? data?.Notifications ?? [];
            renderNotifications(notifications);
        } catch (error) {
            console.error("Notifications loading error:", error);
            list.textContent = "دریافت اعلان‌ها با خطا مواجه شد.";
        }
    }

    async function markAsRead(id) {
        if (!id) return;

        await request(endpoints.markRead(id), {
            method: "POST",
            headers: {
                "RequestVerificationToken": token
            }
        });
    }

    async function handleNotificationClick(id, actionUrl, isRead) {
        try {
            if (!isRead) {
                await markAsRead(id);
                await loadUnreadCount();
            }

            if (actionUrl) {
                const target = new URL(actionUrl, window.location.origin);

                if (target.origin === window.location.origin) {
                    window.location.assign(target.href);
                }
            }

            await loadNotifications();
        } catch (error) {
            console.error("Mark notification as read error:", error);
        }
    }

    async function markAllAsRead() {
        if (!token) {
            console.error("Anti-forgery token was not found.");
            return;
        }

        markAllButton.disabled = true;

        try {
            await request(endpoints.markAll, {
                method: "POST",
                headers: {
                    "RequestVerificationToken": token
                }
            });

            await Promise.all([
                loadUnreadCount(),
                loadNotifications()
            ]);
        } catch (error) {
            console.error("Mark all notifications as read error:", error);
            markAllButton.disabled = false;
        }
    }

    trigger.addEventListener("click", async () => {
        const isOpen = !dropdown.hidden;

        dropdown.hidden = isOpen;
        trigger.setAttribute("aria-expanded", String(!isOpen));

        if (!isOpen) {
            await Promise.all([
                loadNotifications(),
                loadUnreadCount()
            ]);
        }
    });

    markAllButton.addEventListener("click", markAllAsRead);

    document.addEventListener("click", event => {
        if (!container.contains(event.target)) {
            dropdown.hidden = true;
            trigger.setAttribute("aria-expanded", "false");
        }
    });

        loadUnreadCount();
    checkForNewNotifications();

    // Check for new notifications every 15 seconds.
    window.setInterval(checkForNewNotifications, 15000);
})();
