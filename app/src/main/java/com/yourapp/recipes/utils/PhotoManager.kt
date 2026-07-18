package com.yourapp.recipes.utils

import android.content.Context
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.net.Uri
import android.os.Environment
import androidx.core.content.FileProvider
import java.io.File
import java.io.FileOutputStream
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject
import javax.inject.Singleton
import kotlin.math.min

/**
 * Менеджер для работы с фотографиями рецептов.
 * Обеспечивает сохранение, сжатие и управление изображениями.
 */
@Singleton
class PhotoManager @Inject constructor(
    private val context: Context
) {
    companion object {
        private const val PHOTOS_DIR = "recipe_photos"
        private const val MAX_IMAGE_SIZE = 1024 // pixels
        private const val JPEG_QUALITY = 85
        private const val MAX_FILE_SIZE = 500 * 1024 // 500KB
    }
    
    /**
     * Создает файл для сохранения фото с камеры
     */
    fun createImageFile(): File {
        val timeStamp = SimpleDateFormat("yyyyMMdd_HHmmss", Locale.getDefault()).format(Date())
        val imageFileName = "JPEG_${timeStamp}_"
        val storageDir = File(context.filesDir, PHOTOS_DIR)
        
        if (!storageDir.exists()) {
            storageDir.mkdirs()
        }
        
        return File.createTempFile(imageFileName, ".jpg", storageDir)
    }
    
    /**
     * Создает URI для FileProvider
     */
    fun getImageUri(file: File): Uri {
        return FileProvider.getUriForFile(
            context,
            "${context.packageName}.fileprovider",
            file
        )
    }
    
    /**
     * Сжимает и сохраняет изображение из URI
     */
    fun saveCompressedImage(uri: Uri): String? {
        return try {
            val inputStream = context.contentResolver.openInputStream(uri)
            val originalBitmap = BitmapFactory.decodeStream(inputStream)
            inputStream?.close()
            
            val compressedBitmap = compressBitmap(originalBitmap)
            originalBitmap.recycle()
            
            saveBitmapToFile(compressedBitmap)
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
    
    /**
     * Сжимает изображение до нужных размеров
     */
    private fun compressBitmap(bitmap: Bitmap): Bitmap {
        val width = bitmap.width
        val height = bitmap.height
        
        // Проверяем, нужно ли сжатие
        if (width <= MAX_IMAGE_SIZE && height <= MAX_IMAGE_SIZE) {
            return bitmap
        }
        
        // Вычисляем новые размеры
        val ratio = min(
            MAX_IMAGE_SIZE.toFloat() / width,
            MAX_IMAGE_SIZE.toFloat() / height
        )
        
        val newWidth = (width * ratio).toInt()
        val newHeight = (height * ratio).toInt()
        
        return Bitmap.createScaledBitmap(bitmap, newWidth, newHeight, true)
    }
    
    /**
     * Сохраняет Bitmap в файл
     */
    private fun saveBitmapToFile(bitmap: Bitmap): String? {
        return try {
            val file = createImageFile()
            FileOutputStream(file).use { outputStream ->
                var quality = JPEG_QUALITY
                bitmap.compress(Bitmap.CompressFormat.JPEG, quality, outputStream)
                
                // Уменьшаем качество, если файл слишком большой
                while (file.length() > MAX_FILE_SIZE && quality > 20) {
                    quality -= 10
                    outputStream.flush()
                    bitmap.compress(Bitmap.CompressFormat.JPEG, quality, outputStream)
                }
            }
            bitmap.recycle()
            file.absolutePath
        } catch (e: Exception) {
            e.printStackTrace()
            null
        }
    }
    
    /**
     * Загружает Bitmap из файла
     */
    fun loadBitmap(filePath: String): Bitmap? {
        return try {
            BitmapFactory.decodeFile(filePath)
        } catch (e: Exception) {
            null
        }
    }
    
    /**
     * Удаляет файл изображения
     */
    fun deletePhoto(filePath: String): Boolean {
        return try {
            File(filePath).delete()
        } catch (e: Exception) {
            false
        }
    }
    
    /**
     * Получает размер всех фото
     */
    fun getPhotosStorageSize(): Long {
        val storageDir = File(context.filesDir, PHOTOS_DIR)
        return if (storageDir.exists()) {
            getFolderSize(storageDir)
        } else {
            0
        }
    }
    
    /**
     * Очищает все фото
     */
    fun clearAllPhotos() {
        val storageDir = File(context.filesDir, PHOTOS_DIR)
        if (storageDir.exists()) {
            storageDir.deleteRecursively()
        }
    }
    
    private fun getFolderSize(dir: File): Long {
        var size: Long = 0
        dir.listFiles()?.forEach { file ->
            size += if (file.isDirectory) {
                getFolderSize(file)
            } else {
                file.length()
            }
        }
        return size
    }
}
