(() => {
    "use strict";

    const root = document.getElementById("rmoMeeting");

    if (!root) return;

    const meetingTimer = document.getElementById("meetingCountdown");
    const sectionTimer = document.getElementById("sectionCountdown");

    const timerState = (root.dataset.timerState || "").toLowerCase();

    const isRunning =
        timerState === "running" ||
        root.dataset.sessionStatus?.toLowerCase() === "inprogress";

    let meetingRemaining = Number(root.dataset.meetingRemaining || 0);

    const formatTime = (seconds) => {
        const negative = seconds < 0;
        const absolute = Math.abs(seconds);

        const hours = Math.floor(absolute / 3600);
        const minutes = Math.floor((absolute % 3600) / 60);
        const remainingSeconds = absolute % 60;

        const formatted = hours > 0
            ? `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}:${String(remainingSeconds).padStart(2, "0")}`
            : `${String(minutes).padStart(2, "0")}:${String(remainingSeconds).padStart(2, "0")}`;

        return negative ? `+${formatted}` : formatted;
    };

    const updateMeetingTimer = () => {
        if (!meetingTimer) return;

        meetingTimer.textContent = formatTime(meetingRemaining);

        meetingTimer.classList.toggle(
            "is-overtime",
            meetingRemaining < 0
        );
    };

    const updateSectionTimer = () => {
        if (!sectionTimer) return;

        const startedAtValue = sectionTimer.dataset.started;

        if (!startedAtValue) {
            sectionTimer.textContent = "--:--";
            return;
        }

        const startedAt = new Date(startedAtValue).getTime();

        const durationSeconds =
            Number(sectionTimer.dataset.duration || 0) * 60;

        const extendedSeconds =
            Number(sectionTimer.dataset.extended || 0);

        if (!Number.isFinite(startedAt) || !durationSeconds) {
            sectionTimer.textContent = "--:--";
            return;
        }

        const elapsedSeconds = Math.max(
            0,
            Math.floor((Date.now() - startedAt) / 1000)
        );

        const remaining =
            durationSeconds + extendedSeconds - elapsedSeconds;

        sectionTimer.textContent = formatTime(remaining);

        sectionTimer.classList.toggle(
            "is-overtime",
            remaining < 0
        );
    };

    updateMeetingTimer();
    updateSectionTimer();

    if (isRunning) {
        window.setInterval(() => {
            meetingRemaining--;

            updateMeetingTimer();
            updateSectionTimer();
        }, 1000);
    }
})();