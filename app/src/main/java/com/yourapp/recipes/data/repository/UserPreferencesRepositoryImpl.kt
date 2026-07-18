package com.yourapp.recipes.data.repository

import com.yourapp.recipes.data.local.dao.UserPreferencesDao
import com.yourapp.recipes.data.local.database.entity.UserPreferencesEntity
import com.yourapp.recipes.domain.model.MeasurementSystem
import com.yourapp.recipes.domain.model.ThemeMode
import com.yourapp.recipes.domain.model.UserPreferences
import com.yourapp.recipes.domain.repository.UserPreferencesRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class UserPreferencesRepositoryImpl @Inject constructor(
    private val userPreferencesDao: UserPreferencesDao
) : UserPreferencesRepository {
    
    override fun getPreferences(): Flow<UserPreferences> {
        return userPreferencesDao.getPreferences().map { entity ->
            entity?.toDomain() ?: UserPreferences()
        }
    }
    
    override suspend fun updateThemeMode(themeMode: String) {
        userPreferencesDao.updateThemeMode(themeMode)
    }
    
    override suspend fun updateMeasurementSystem(system: String) {
        userPreferencesDao.updateMeasurementSystem(system)
    }
    
    override suspend fun savePreferences(preferences: UserPreferences) {
        userPreferencesDao.savePreferences(preferences.toEntity())
    }
    
    private fun UserPreferencesEntity.toDomain(): UserPreferences {
        return UserPreferences(
            themeMode = try {
                ThemeMode.valueOf(themeMode.uppercase())
            } catch (e: Exception) {
                ThemeMode.SYSTEM
            },
            defaultServings = defaultServings,
            measurementSystem = try {
                MeasurementSystem.valueOf(measurementSystem.uppercase())
            } catch (e: Exception) {
                MeasurementSystem.METRIC
            },
            shoppingListReminder = shoppingListReminder,
            reminderDays = reminderDays.split(",").mapNotNull { it.trim().toIntOrNull() }.toSet(),
            reminderTime = reminderTime,
            autoBackup = autoBackup,
            lastBackupDate = lastBackupDate,
            language = language
        )
    }
    
    private fun UserPreferences.toEntity(): UserPreferencesEntity {
        return UserPreferencesEntity(
            themeMode = themeMode.name.lowercase(),
            defaultServings = defaultServings,
            measurementSystem = measurementSystem.name.lowercase(),
            shoppingListReminder = shoppingListReminder,
            reminderDays = reminderDays.joinToString(","),
            reminderTime = reminderTime,
            autoBackup = autoBackup,
            lastBackupDate = lastBackupDate,
            language = language
        )
    }
}
