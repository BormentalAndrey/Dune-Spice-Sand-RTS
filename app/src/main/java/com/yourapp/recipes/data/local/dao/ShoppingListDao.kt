package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.ShoppingItemEntity
import kotlinx.coroutines.flow.Flow

data class CategorySpending(
    @ColumnInfo(name = "category") val category: String,
    @ColumnInfo(name = "total") val total: Float
)

@Dao
interface ShoppingListDao {
    
    @Query("SELECT * FROM shopping_items ORDER BY category ASC, is_purchased ASC, name ASC")
    fun getAllItems(): Flow<List<ShoppingItemEntity>>
    
    @Query("SELECT * FROM shopping_items WHERE is_purchased = 0 ORDER BY category ASC")
    fun getUnpurchasedItems(): Flow<List<ShoppingItemEntity>>
    
    @Query("SELECT DISTINCT category FROM shopping_items ORDER BY category")
    fun getCategories(): Flow<List<String>>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertItem(item: ShoppingItemEntity): Long
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertItems(items: List<ShoppingItemEntity>)
    
    @Update
    suspend fun updateItem(item: ShoppingItemEntity)
    
    @Query("UPDATE shopping_items SET is_purchased = :isPurchased, purchase_date = CASE WHEN :isPurchased = 1 THEN :purchaseDate ELSE purchase_date END WHERE id = :itemId")
    suspend fun updatePurchasedStatus(itemId: Long, isPurchased: Boolean, purchaseDate: Long = System.currentTimeMillis())
    
    @Query("UPDATE shopping_items SET price = :price WHERE id = :itemId")
    suspend fun updateItemPrice(itemId: Long, price: Float)
    
    @Query("UPDATE shopping_items SET is_purchased = 0")
    suspend fun resetAllItems()
    
    @Delete
    suspend fun deleteItem(item: ShoppingItemEntity)
    
    @Query("DELETE FROM shopping_items WHERE is_purchased = 1")
    suspend fun deletePurchasedItems()
    
    @Query("DELETE FROM shopping_items")
    suspend fun deleteAllItems()
    
    @Query("SELECT COALESCE(SUM(price * quantity), 0) FROM shopping_items WHERE is_purchased = 0")
    fun getTotalRemainingPrice(): Flow<Float>
    
    @Query("SELECT COALESCE(SUM(price * quantity), 0) FROM shopping_items WHERE is_purchased = 1 AND purchase_date >= :startDate")
    fun getTotalSpentSince(startDate: Long): Flow<Float>
    
    @Query("SELECT category, SUM(price * quantity) as total FROM shopping_items WHERE is_purchased = 1 AND purchase_date >= :startDate GROUP BY category ORDER BY total DESC")
    fun getSpendingByCategory(startDate: Long): Flow<List<CategorySpending>>
}
