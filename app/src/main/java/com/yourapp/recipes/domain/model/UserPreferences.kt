package com.yourapp.recipes.domain.model

data class UserPreferences(
    val themeMode: ThemeMode = ThemeMode.SYSTEM,
    val defaultServings: Int = 2,
    val measurementSystem: MeasurementSystem = MeasurementSystem.METRIC,
    val shoppingListReminder: Boolean = true,
    val reminderDays: Set<Int> = setOf(1, 3, 5),
    val reminderTime: String = "09:00",
    val autoBackup: Boolean = false,
    val lastBackupDate: Long = 0,
    val language: String = "ru"
)

enum class ThemeMode {
    LIGHT, DARK, SYSTEM
}

enum class MeasurementSystem(val displayName: String) {
    METRIC("Метрическая (г, мл)"),
    IMPERIAL("Имперская (oz, cups)")
}
