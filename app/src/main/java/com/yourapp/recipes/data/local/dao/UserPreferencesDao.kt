package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.UserPreferencesEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface UserPreferencesDao {
    
    @Query("SELECT * FROM user_preferences WHERE id = 1")
    fun getPreferences(): Flow<UserPreferencesEntity?>
    
    @Query("SELECT * FROM user_preferences WHERE id = 1")
    suspend fun getPreferencesSync(): UserPreferencesEntity?
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun savePreferences(preferences: UserPreferencesEntity)
    
    @Query("UPDATE user_preferences SET theme_mode = :themeMode WHERE id = 1")
    suspend fun updateThemeMode(themeMode: String)
    
    @Query("UPDATE user_preferences SET measurement_system = :system WHERE id = 1")
    suspend fun updateMeasurementSystem(system: String)
    
    @Query("UPDATE user_preferences SET auto_backup = :autoBackup WHERE id = 1")
    suspend fun updateAutoBackup(autoBackup: Boolean)
}
