(() => {
    "use strict";

    const root = document.getElementById("liveMeetingRoom");

    if (!root || !window.signalR) {
        return;
    }

    const meetingId = Number(root.dataset.meetingId);
    const hubUrl = root.dataset.hubUrl;
    const canControl = root.dataset.canControl === "true";

    const elements = {
        meetingTimer: document.getElementById("meetingTimer"),
        meetingElapsed: document.getElementById("meetingElapsed"),
        meetingProgress: document.getElementById("meetingProgress"),
        meetingStatus: document.getElementById("meetingStatus"),
        timerStateBadge: document.getElementById("timerStateBadge"),
        meetingTimerMessage: document.getElementById("meetingTimerMessage"),

        currentAgendaTitle: document.getElementById("currentAgendaTitle"),
        sectionTimer: document.getElementById("sectionTimer"),
        sectionStatusBadge: document.getElementById("sectionStatusBadge"),
        sectionTimerCaption: document.getElementById("sectionTimerCaption"),
        sectionDuration: document.getElementById("sectionDuration"),
        sectionPresenter: document.getElementById("sectionPresenter"),
        sectionOvertimeMessage: document.getElementById("sectionOvertimeMessage"),

        agendaList: document.getElementById("agendaList"),
        agendaCount: document.getElementById("agendaCount"),

        connectionIndicator: document.getElementById("connectionIndicator"),
        connectionText: document.getElementById("connectionText"),

        liveRoomFeedback: document.getElementById("liveRoomFeedback"),
        serverClock: document.getElementById("serverClock")
    };

    let connection = null;
    let snapshot = null;

    let serverOffsetMilliseconds = 0;
    let snapshotReceivedAt = 0;

    let meetingElapsedAtSnapshot = 0;
    let sectionRemainingAtSnapshot = null;

    let selectedAgendaId = null;
    let isExecutingCommand = false;
    let isStartingConnection = false;

    const timerStates = {
        NotStarted: "Not started",
        Running: "Running",
        Paused: "Paused",
        Completed: "Completed"
    };

    function pad(value) {
        return String(value).padStart(2, "0");
    }

    function formatDuration(totalSeconds, allowNegative = false) {
        if (totalSeconds === null ||
            totalSeconds === undefined) {
            return "--:--";
        }

        const negative = totalSeconds < 0;
        const absolute = Math.abs(Math.floor(totalSeconds));

        const hours = Math.floor(absolute / 3600);
        const minutes = Math.floor((absolute % 3600) / 60);
        const seconds = absolute % 60;

        const result = hours > 0
            ? `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`
            : `${pad(minutes)}:${pad(seconds)}`;

        if (negative && allowNegative) {
            return `+${result}`;
        }

        return result;
    }

    function getServerNow() {
        return Date.now() + serverOffsetMilliseconds;
    }

    function getElapsedSeconds() {
        if (!snapshot) {
            return 0;
        }

        if (snapshot.timerState !== "Running") {
            return snapshot.elapsedSeconds;
        }

        const elapsedSinceSnapshot =
            Math.max(
                0,
                getServerNow() - snapshotReceivedAt
            ) / 1000;

        return Math.floor(
            meetingElapsedAtSnapshot + elapsedSinceSnapshot
        );
    }

    function getMeetingRemainingSeconds() {
        if (!snapshot) {
            return 0;
        }

        const totalSeconds =
            snapshot.plannedDurationMinutes * 60;

        return Math.max(
            0,
            totalSeconds - getElapsedSeconds()
        );
    }

    function getSectionRemainingSeconds() {
        if (!snapshot ||
            sectionRemainingAtSnapshot === null ||
            sectionRemainingAtSnapshot === undefined) {
            return null;
        }

        // تایمر بخش فقط در زمان Running جلسه حرکت می‌کند.
        if (snapshot.timerState !== "Running") {
            return sectionRemainingAtSnapshot;
        }

        const elapsedSinceSnapshot =
            Math.max(
                0,
                getServerNow() - snapshotReceivedAt
            ) / 1000;

        return Math.ceil(
            sectionRemainingAtSnapshot - elapsedSinceSnapshot
        );
    }

    function setConnectionState(connected, message) {
        elements.connectionIndicator?.classList.toggle(
            "is-connected",
            connected
        );

        elements.connectionIndicator?.classList.toggle(
            "is-disconnected",
            !connected
        );

        if (elements.connectionText) {
            elements.connectionText.textContent = message;
        }
    }

    function showFeedback(message, isError = false) {
        if (!elements.liveRoomFeedback) {
            return;
        }

        elements.liveRoomFeedback.textContent = message;

        elements.liveRoomFeedback.classList.toggle(
            "is-error",
            isError
        );
    }

    function updateSnapshot(data) {
        if (!data) {
            return;
        }

        snapshot = data;

        const serverTime = new Date(data.serverTimeUtc).getTime();

        if (!Number.isNaN(serverTime)) {
            serverOffsetMilliseconds =
                serverTime - Date.now();
        }

        snapshotReceivedAt = getServerNow();

        meetingElapsedAtSnapshot =
            data.elapsedSeconds ?? 0;

        sectionRemainingAtSnapshot =
            data.currentAgendaItemRemainingSeconds;

        selectedAgendaId =
            data.currentAgendaItemId ?? selectedAgendaId;

        renderSnapshot();
    }

    function renderSnapshot() {
        if (!snapshot) {
            return;
        }

        if (elements.meetingStatus) {
            elements.meetingStatus.textContent =
                snapshot.meetingStatus;
        }

        if (elements.timerStateBadge) {
            elements.timerStateBadge.textContent =
                timerStates[snapshot.timerState] ??
                snapshot.timerState;
        }

        renderTimers();
        renderAgenda();
        renderMeetingMessage();
    }

    function renderTimers() {
        if (!snapshot) {
            return;
        }

        const remaining = getMeetingRemainingSeconds();
        const elapsed = getElapsedSeconds();

        if (elements.meetingTimer) {
            elements.meetingTimer.textContent =
                formatDuration(remaining);
        }

        if (elements.meetingElapsed) {
            elements.meetingElapsed.textContent =
                `Elapsed: ${formatDuration(elapsed)}`;
        }

        const plannedSeconds =
            snapshot.plannedDurationMinutes * 60;

        const progress = plannedSeconds > 0
            ? Math.min(100, (elapsed / plannedSeconds) * 100)
            : 0;

        if (elements.meetingProgress) {
            elements.meetingProgress.style.width =
                `${progress}%`;
        }

        if (elements.currentAgendaTitle) {
            elements.currentAgendaTitle.textContent =
                snapshot.currentAgendaItemTitle ??
                "No active agenda item";
        }

        const sectionRemaining =
            getSectionRemainingSeconds();

        if (elements.sectionTimer) {
            elements.sectionTimer.textContent =
                formatDuration(sectionRemaining, true);
        }

        if (elements.sectionStatusBadge) {
            elements.sectionStatusBadge.textContent =
                snapshot.currentAgendaItemStatus ??
                "Not started";
        }

        if (elements.sectionDuration &&
            snapshot.currentAgendaItemDurationSeconds !== null &&
            snapshot.currentAgendaItemDurationSeconds !== undefined) {

            elements.sectionDuration.textContent =
                `Duration: ${formatDuration(
                    snapshot.currentAgendaItemDurationSeconds
                )}`;
        }

        if (elements.sectionOvertimeMessage) {
            elements.sectionOvertimeMessage.textContent =
                sectionRemaining !== null &&
                sectionRemaining < 0
                    ? "Section is in overtime"
                    : "";
        }

        if (elements.sectionTimerCaption) {
            if (!snapshot.currentAgendaItemId) {
                elements.sectionTimerCaption.textContent =
                    "Waiting for section";
            } else if (snapshot.timerState === "Paused") {
                elements.sectionTimerCaption.textContent =
                    "Section timer paused";
            } else {
                elements.sectionTimerCaption.textContent =
                    "Current section remaining time";
            }
        }

        if (elements.serverClock) {
            elements.serverClock.textContent =
                `Server time: ${new Date(getServerNow())
                    .toLocaleTimeString()}`;
        }
    }

    function renderMeetingMessage() {
        if (!elements.meetingTimerMessage || !snapshot) {
            return;
        }

        switch (snapshot.timerState) {
            case "NotStarted":
                elements.meetingTimerMessage.textContent =
                    "Waiting for meeting";
                break;

            case "Running":
                elements.meetingTimerMessage.textContent =
                    "Meeting is in progress";
                break;

            case "Paused":
                elements.meetingTimerMessage.textContent =
                    "Meeting is paused";
                break;

            case "Completed":
                elements.meetingTimerMessage.textContent =
                    "Meeting completed";
                break;

            default:
                elements.meetingTimerMessage.textContent = "";
        }
    }

    function renderAgenda() {
        if (!elements.agendaList || !snapshot) {
            return;
        }

        const items = snapshot.agendaItems ?? [];

        if (elements.agendaCount) {
            elements.agendaCount.textContent =
                `${items.length} items`;
        }

        elements.agendaList.innerHTML = "";

        items.forEach(item => {
            const wrapper = document.createElement("div");
            wrapper.className = "crm-agenda-item";

            if (item.status === "InProgress" ||
                item.status === "Overtime") {
                wrapper.classList.add("is-active");
            }

            if (item.status === "Completed") {
                wrapper.classList.add("is-completed");
            }

            const order = document.createElement("div");
            order.className = "crm-agenda-order";
            order.textContent = String(item.sortOrder + 1);

            const content = document.createElement("div");
            content.className = "crm-agenda-item-content";

            const title = document.createElement("div");
            title.className = "crm-agenda-item-title";
            title.textContent = item.title;

            const meta = document.createElement("div");
            meta.className = "crm-agenda-item-meta";

            const type = document.createElement("span");
            type.textContent = item.type;

            const duration = document.createElement("span");
            duration.textContent = `${item.durationMinutes} min`;

            const status = document.createElement("span");
            status.textContent = item.status;

            meta.append(type, duration, status);
            content.append(title, meta);

            const state = document.createElement("div");
            state.className = "crm-agenda-item-state";
            state.textContent = item.status;

            wrapper.append(order, content, state);

            elements.agendaList.appendChild(wrapper);
        });

        const selector =
            document.getElementById("agendaItemSelector");

        if (selector && selectedAgendaId) {
            selector.value = String(selectedAgendaId);
        }
    }

    async function invoke(method, ...args) {
        if (!connection ||
            connection.state !== signalR.HubConnectionState.Connected) {
            showFeedback("Connection is not available.", true);
            return;
        }

        if (isExecutingCommand) {
            return;
        }

        isExecutingCommand = true;
        setControlButtonsDisabled(true);

        try {
            const result = await connection.invoke(
                method,
                ...args
            );

            if (result) {
                updateSnapshot(result);
            }

            showFeedback("Operation completed.");
        } catch (error) {
            console.error(error);

            showFeedback(
                error.message || "Operation failed.",
                true
            );
        } finally {
            isExecutingCommand = false;
            setControlButtonsDisabled(false);
        }
    }

    function setControlButtonsDisabled(disabled) {
        document.querySelectorAll(
            ".crm-control-button"
        ).forEach(button => {
            button.disabled = disabled;
        });
    }

    function getSelectedAgendaId() {
        const selector =
            document.getElementById("agendaItemSelector");

        const value = Number(selector?.value);

        if (!value || value <= 0) {
            showFeedback(
                "Select an agenda item first.",
                true
            );

            return null;
        }

        return value;
    }

    function bindControls() {
        if (!canControl) {
            return;
        }

        document.querySelectorAll("[data-action]")
            .forEach(button => {
                button.addEventListener("click", async () => {
                    const action = button.dataset.action;

                    switch (action) {
                        case "start-meeting":
                            await invoke(
                                "StartMeetingTimer",
                                meetingId
                            );
                            break;

                        case "pause-meeting":
                            await invoke(
                                "PauseMeetingTimer",
                                meetingId
                            );
                            break;

                        case "resume-meeting":
                            await invoke(
                                "ResumeMeetingTimer",
                                meetingId
                            );
                            break;

                        case "complete-meeting":
                            if (confirm(
                                "Are you sure you want to end this meeting?"
                            )) {
                                await invoke(
                                    "CompleteMeetingTimer",
                                    meetingId
                                );
                            }
                            break;

                        case "start-agenda": {
                            const id = getSelectedAgendaId();

                            if (id) {
                                await invoke(
                                    "StartAgendaItem",
                                    meetingId,
                                    id
                                );
                            }

                            break;
                        }

                        case "complete-agenda": {
                            const id = getSelectedAgendaId();

                            if (id) {
                                await invoke(
                                    "CompleteAgendaItem",
                                    meetingId,
                                    id
                                );
                            }

                            break;
                        }

                        case "skip-agenda": {
                            const id = getSelectedAgendaId();

                            if (id) {
                                await invoke(
                                    "SkipAgendaItem",
                                    meetingId,
                                    id
                                );
                            }

                            break;
                        }

                        case "extend-agenda": {
                            const id = getSelectedAgendaId();

                            if (id) {
                                await invoke(
                                    "ExtendAgendaItem",
                                    meetingId,
                                    id,
                                    60
                                );
                            }

                            break;
                        }
                    }
                });
            });
    }

    function bindFullscreen() {
        const button =
            document.getElementById("fullscreenButton");

        button?.addEventListener("click", async () => {
            try {
                if (!document.fullscreenElement) {
                    await root.requestFullscreen();
                } else {
                    await document.exitFullscreen();
                }
            } catch (error) {
                console.error(error);
            }
        });
    }

    function delay(milliseconds) {
        return new Promise(resolve =>
            window.setTimeout(resolve, milliseconds)
        );
    }

    async function startConnectionWithRetry() {
        if (isStartingConnection || !connection) {
            return;
        }

        isStartingConnection = true;

        let retryDelay = 1000;
        const maxRetryDelay = 15000;

        try {
            while (
                connection.state ===
                signalR.HubConnectionState.Disconnected
            ) {
                try {
                    setConnectionState(
                        false,
                        "Connecting..."
                    );

                    await connection.start();

                    return;
                } catch (error) {
                    console.error(
                        "SignalR connection attempt failed:",
                        error
                    );

                    setConnectionState(
                        false,
                        "Connection failed. Retrying..."
                    );

                    await delay(retryDelay);

                    retryDelay = Math.min(
                        retryDelay * 2,
                        maxRetryDelay
                    );
                }
            }
        } finally {
            isStartingConnection = false;
        }
    }

    async function joinMeeting() {
        if (!connection ||
            connection.state !== signalR.HubConnectionState.Connected) {
            return;
        }

        try {
            const initialSnapshot = await connection.invoke(
                "JoinMeeting",
                meetingId
            );

            updateSnapshot(initialSnapshot);

            setConnectionState(true, "Connected");
        } catch (error) {
            console.error("JoinMeeting failed:", error);

            setConnectionState(false, "Access denied");

            showFeedback(
                error.message || "Unable to join meeting.",
                true
            );
        }
    }

    async function connect() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect([
                0,
                2000,
                5000,
                10000,
                20000
            ])
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        connection.on(
            "MeetingSnapshotUpdated",
            updateSnapshot
        );

        connection.onreconnecting(() => {
            setConnectionState(
                false,
                "Reconnecting..."
            );
        });

        connection.onreconnected(async () => {
            setConnectionState(
                true,
                "Connected"
            );

            await joinMeeting();
        });

        connection.onclose(() => {
            setConnectionState(
                false,
                "Disconnected. Reconnecting..."
            );

            // Automatic reconnect تمام شده است.
            // اتصال اولیه را دوباره برقرار می‌کنیم.
            void startConnectionWithRetry().then(async () => {
                if (connection.state ===
                    signalR.HubConnectionState.Connected) {
                    await joinMeeting();
                }
            });
        });

        await startConnectionWithRetry();

        if (connection.state ===
            signalR.HubConnectionState.Connected) {
            await joinMeeting();
        }
    }

    bindControls();
    bindFullscreen();

    void connect();

    window.setInterval(() => {
        if (snapshot) {
            renderTimers();
        }
    }, 250);
})();