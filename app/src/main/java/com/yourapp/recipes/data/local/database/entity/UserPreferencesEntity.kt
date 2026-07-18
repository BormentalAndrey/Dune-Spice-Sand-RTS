package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

/**
 * Сущность пользовательских настроек.
 * Хранит все настройки приложения в одной таблице.
 */
@Entity(tableName = "user_preferences")
data class UserPreferencesEntity(
    @PrimaryKey
    val id: Int = 1,
    
    @ColumnInfo(name = "theme_mode")
    val themeMode: String = "system", // system, light, dark
    
    @ColumnInfo(name = "default_servings")
    val defaultServings: Int = 2,
    
    @ColumnInfo(name = "measurement_system")
    val measurementSystem: String = "metric", // metric, imperial
    
    @ColumnInfo(name = "shopping_list_reminder")
    val shoppingListReminder: Boolean = true,
    
    @ColumnInfo(name = "reminder_days")
    val reminderDays: String = "1,3,5", // comma-separated days
    
    @ColumnInfo(name = "reminder_time")
    val reminderTime: String = "09:00",
    
    @ColumnInfo(name = "auto_backup")
    val autoBackup: Boolean = false,
    
    @ColumnInfo(name = "last_backup_date")
    val lastBackupDate: Long = 0,
    
    @ColumnInfo(name = "language")
    val language: String = "ru"
)
